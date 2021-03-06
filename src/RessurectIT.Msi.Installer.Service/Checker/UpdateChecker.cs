﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using RessurectIT.Msi.Installer.Gatherer.Dto;
using RessurectIT.Msi.Installer.Installer.Dto;
using static RessurectIT.Msi.Installer.Program;
using static RessurectIT.Msi.Installer.Installer.WindowsInstaller;

namespace RessurectIT.Msi.Installer.Checker
{
    /// <summary>
    /// Class used for checking availability of updates
    /// </summary>
    [ExportEx]
    public class UpdateChecker : IDisposable
    {
        #region constants

        /// <summary>
        /// Install operation
        /// </summary>
        private const string InstallOperation = "install";

        /// <summary>
        /// Notify operation
        /// </summary>
        private const string NotifyOperation = "notify";
        #endregion


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
        /// Service used for stopping windows service
        /// </summary>
        private readonly StopService.StopService _stopService;

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
        /// <param name="stopService">Service used for stopping windows service</param>
        public UpdateChecker(IServiceProvider serviceProvider,
                             ILogger<UpdateChecker> logger,
                             ServiceConfig config,
                             StopService.StopService stopService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timer = new Timer(config.CheckInterval);
            _config = config;
            _stopService = stopService;

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
            MsiUpdate[] newUpdates = gatherer.CheckForUpdates<MsiUpdate>();

            IMsiUpdate[] autoInstallUpdates = newUpdates
                .Where(update => _config.AutoInstall || update.AutoInstall.HasValue && update.AutoInstall.Value)
                .ToArray();

            IMsiUpdate[] notifyUpdates = newUpdates
                .Where(update => !_config.AutoInstall && (!update.AutoInstall.HasValue || !update.AutoInstall.Value) && update.Notify.HasValue && update.Notify.Value)
                .ToArray();

            _logger.LogDebug("Found auto install updates: {@updates}", (object)autoInstallUpdates);

            foreach (IMsiUpdate update in autoInstallUpdates)
            {
                if (_config.LocalServer)
                {
                    if (!await Install(update, InstallOperation))
                    {
                        break;
                    }
                }
                else
                {
                    if (!InstallAsLoggedUser(update, InstallOperation))
                    {
                        break;
                    }
                }
            }

            _logger.LogDebug("Found notify updates: {@updates}", (object)notifyUpdates);

            foreach (IMsiUpdate update in notifyUpdates)
            {
                if (_config.LocalServer)
                {
                    if (!await Install(update, NotifyOperation))
                    {
                        break;
                    }
                }
                else
                {
                    if (!InstallAsLoggedUser(update, NotifyOperation))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Installs update as logged user
        /// </summary>
        /// <param name="update">Update to be installed</param>
        /// <param name="operation">Type of operation that should be called (install|notify)</param>
        private bool InstallAsLoggedUser(IMsiUpdate update, string operation)
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

                return true;
            }

            bool result = WinApi.OpenProcessToken(runningProcess.Handle, WinApi.TOKEN_ALL_ACCESS, out IntPtr token);

            if (!result)
            {
                _logger.LogError("Failed to obtain user token, error code '{code}'", WinApi.GetLastError());

                return true;
            }

            string cmdLine = Path.Combine(Directory.GetCurrentDirectory(), @$"RessurectIT.Msi.Installer.exe --{operation} ""{SerializeUpdate(update, _jsonSerializerSettings)}""");

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

            _logger.LogDebug("New process as logged user started successfully: {result}", result);

            if (!result)
            {
                _logger.LogError("Failed to create process as user, error code '{code}'", WinApi.GetLastError());

                return true;
            }

            result = WinApi.CloseHandle(token);

            if (!result)
            {
                _logger.LogError("Failed to close handle for user token, error code '{code}'", WinApi.GetLastError());

                return true;
            }

            result = WinApi.CloseHandle(pi.hThread);

            if (!result)
            {
                _logger.LogError("Failed to close handle for pi, error code '{code}'", WinApi.GetLastError());

                return true;
            }

            result = WinApi.CloseHandle(pi.hProcess);

            if (!result)
            {
                _logger.LogError("Failed to close handle for pi, error code '{code}'", WinApi.GetLastError());
            }

            //stop service in case of self upgrade
            if (IsRessurectITMsiInstallerMsi(update) && Assembly.GetExecutingAssembly().GetName().Version < new Version(update.Version))
            {
                _logger.LogInformation("Self upgrade detected, shutting down service!");

                _stopService.StopCallback?.Invoke();

                return false;
            }

            return true;
        }

        /// <summary>
        /// Installs update as current user
        /// </summary>
        /// <param name="update">Update to be installed</param>
        /// <param name="operation">Type of operation that should be called (install|notify)</param>
        private async Task<bool> Install(IMsiUpdate update, string operation)
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(Directory.GetCurrentDirectory(), "RessurectIT.Msi.Installer.exe"),
                    Arguments = $@"--{operation} ""{SerializeUpdate(update, _jsonSerializerSettings)}"""
                }
            };

            bool result = process.Start();

            //stop service in case of self upgrade
            if (IsRessurectITMsiInstallerMsi(update) && Assembly.GetExecutingAssembly().GetName().Version < new Version(update.Version))
            {
                _logger.LogInformation("Self upgrade detected, shutting down service!");

                _stopService.StopCallback?.Invoke();

                return false;
            }
            
            _logger.LogDebug("New process started successfully: {result}", result);

            await Task.Factory.StartNew(() => process.WaitForExit(_config.MsiInstallTimeout));

            return true;
        }
        #endregion
    }
}