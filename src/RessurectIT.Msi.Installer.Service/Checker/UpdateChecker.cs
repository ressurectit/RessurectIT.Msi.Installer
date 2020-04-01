using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using DryIocAttributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RessurectIT.Msi.Installer.Configuration;
using RessurectIT.Msi.Installer.Gatherer;
using RessurectIT.Msi.Installer.Installer.Dto;
using static RessurectIT.Msi.Installer.Program;

namespace RessurectIT.Msi.Installer.Checker
{
    /// <summary>
    /// Class used for checking availability of updates
    /// </summary>
    [ExportEx]
    public class UpdateChecker : IDisposable
    {
        #region private fields

        /// <summary>
        /// Instance of timer used for checking for updates
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Service provider used for obtaining dependencies
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<UpdateChecker> _logger;

        /// <summary>
        /// Service configuration
        /// </summary>
        private readonly ServiceConfig _config;

        /// <summary>
        /// Serializer settings used for serialization data to response
        /// </summary>
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="UpdateChecker"/>
        /// </summary>
        /// <param name="serviceProvider">Service provider used for obtaining dependencies</param>
        /// <param name="logger">Logger used for logging</param>
        /// <param name="config">Service configuration</param>
        public UpdateChecker(IServiceProvider serviceProvider,
                             ILogger<UpdateChecker> logger,
                             ServiceConfig config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timer = new Timer(config.CheckInterval);
            _config = config;

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
        }
        #endregion


        #region public methods

        /// <summary>
        /// Starts update checker that checks for updates
        /// </summary>
        public void Start()
        {
            _timer.Elapsed += (sender, args) => DoCheck();
            _timer.AutoReset = true;

            DoCheck();
            _timer.Start();
        }

        /// <summary>
        /// Stops update checker that checks for updates
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }
        #endregion


        #region public methods - Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _timer.Dispose();
        }
        #endregion


        #region private methods

        /// <summary>
        /// Performs check of updates
        /// </summary>
        private async void DoCheck()
        {
            _logger.LogDebug("Checking for updates! Machine: '{MachineName}'");

            using IServiceScope scope = _serviceProvider.CreateScope();

            HttpGatherer gatherer = scope.ServiceProvider.GetService<HttpGatherer>();
            IMsiUpdate[] newUpdates = gatherer.CheckForUpdates(true);

            foreach (IMsiUpdate update in newUpdates)
            {
                if (_config.LocalServer)
                {
                    await Install(update);
                }
                else
                {
                    InstallAsLoggedUser(update);
                }
            }
        }

        /// <summary>
        /// Installs update as logged user
        /// </summary>
        /// <param name="update">Update to be installed</param>
        private void InstallAsLoggedUser(IMsiUpdate update)
        {
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);

            STARTUPINFO si = new STARTUPINFO();

            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = string.Empty;

            Process runningProcess = Process.GetProcesses().SingleOrDefault(process => process.ProcessName == "explorer");

            if (runningProcess == null)
            {
                _logger.LogWarning("There is no available user logged in for '{updateId}' update", update.Id);

                return;
            }

            bool result = WinApi.OpenProcessToken(runningProcess.Handle, WinApi.TOKEN_ALL_ACCESS, out IntPtr token);

            if (!result)
            {
                _logger.LogError("Failed to obtain user token, error code '{code}'", WinApi.GetLastError());

                return;
            }

            string cmdLine = Path.Combine(Directory.GetCurrentDirectory(), @$"RessurectIT.Msi.Installer.exe --request ""{SerializeUpdate(update, _jsonSerializerSettings)}""");

            result = WinApi.CreateProcessAsUser(token,
                                                null,
                                                cmdLine,
                                                ref sa,
                                                ref sa,
                                                false,
                                                0,
                                                IntPtr.Zero,
                                                null,
                                                ref si,
                                                out PROCESS_INFORMATION pi);

            if (!result)
            {
                _logger.LogError("Failed to create process as user, error code '{code}'", WinApi.GetLastError());

                return;
            }

            result = WinApi.CloseHandle(token);

            if (!result)
            {
                _logger.LogError("Failed to close handle for user token, error code '{code}'", WinApi.GetLastError());

                return;
            }

            result = WinApi.CloseHandle(pi.hThread);

            if (!result)
            {
                _logger.LogError("Failed to close handle for pi, error code '{code}'", WinApi.GetLastError());

                return;
            }

            result = WinApi.CloseHandle(pi.hProcess);

            if (!result)
            {
                _logger.LogError("Failed to close handle for pi, error code '{code}'", WinApi.GetLastError());
            }
        }

        /// <summary>
        /// Installs update as current user
        /// </summary>
        /// <param name="update">Update to be installed</param>
        private async Task Install(IMsiUpdate update)
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(Directory.GetCurrentDirectory(), "RessurectIT.Msi.Installer.exe"),
                    Arguments = $@"--request ""{SerializeUpdate(update, _jsonSerializerSettings)}"""
                }
            };

            process.Start();
            
            await Task.Factory.StartNew(() => process.WaitForExit(_config.MsiInstallTimeout));
        }
        #endregion
    }
}