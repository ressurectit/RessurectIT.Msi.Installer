using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DryIocAttributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RessurectIT.Msi.Installer.Configuration;
using RessurectIT.Msi.Installer.Installer.Dto;
using StopServiceClass = RessurectIT.Msi.Installer.StopService.StopService;
using RessurectIT.Msi.Installer.UpdatesDatabase;
using static RessurectIT.Msi.Installer.App;

namespace RessurectIT.Msi.Installer.Installer
{
    /// <summary>
    /// Class used for handling installations
    /// </summary>
    [ExportEx]
    [CurrentScopeReuse]
    public class Installer
    {
        #region private fields

        /// <summary>
        /// Application configuration
        /// </summary>
        private readonly ConfigBase _config;

        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<Installer> _logger;

        /// <summary>
        /// Service used for stopping windows service
        /// </summary>
        private readonly StopServiceClass _stopService;

        /// <summary>
        /// MSI installer instance
        /// </summary>
        private readonly WindowsInstaller _msiInstaller;

        /// <summary>
        /// Used for managing installed updates
        /// </summary>
        private readonly IUpdatesDatabase _updatesDatabase;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="Installer"/>
        /// </summary>
        /// <param name="config">Application configuration</param>
        /// <param name="logger">Logger used for logging</param>
        /// <param name="stopService">Service used for stopping windows service</param>
        /// <param name="msiInstaller">MSI installer instance</param>
        /// <param name="updatesDatabase">Used for managing installed updates</param>
        public Installer(ConfigBase config,
                         ILogger<Installer> logger,
                         StopServiceClass stopService,
                         WindowsInstaller msiInstaller,
                         IUpdatesDatabase updatesDatabase)
        {
            _config = config;
            _logger = logger;
            _stopService = stopService;
            _msiInstaller = msiInstaller;
            _updatesDatabase = updatesDatabase;
        }
        #endregion


        #region public methods

        /// <summary>
        /// Installs msi updates
        /// </summary>
        /// <param name="updates">MSI updates to be installed</param>
        public void Install(params IMsiUpdate[] updates)
        {
            _logger.LogDebug("Installing updates {@updates} Machine: '{MachineName}'", (object)updates);

            foreach (IMsiUpdate update in updates)
            {
                bool @break = false;

                _logger.LogInformation($"Installing update for {update.Id}, version {update.Version}, from {Path.GetFileName(update.MsiPath)}. Machine: '{{MachineName}}'");

                try
                {
                    StopProcess(update);
                    _msiInstaller.Uninstall(update);
                    _msiInstaller.Install(update,
                                          () =>
                                          {
                                              @break = true;

                                              _stopService.StopCallback?.Invoke();
                                          });

                    if (@break)
                    {
                        break;
                    }
                }
                catch (InstallationException ex)
                {
                    _logger.LogError(ex, "Failed to install new update! Machine: '{MachineName}'");

                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to install new update! Machine: '{MachineName}'");

                    continue;
                }

                //stores to installed updates
                update.UninstallProductCode = WindowsInstaller.GetProductCode(update.MsiPath);

                _updatesDatabase.SetInstalledUpdates(update);

                _logger.LogInformation($"Installation of update for '{update.Id}' version '{update.Version}' was successful. Machine: '{{MachineName}}'");
            }
        }
        #endregion


        #region internal methods

        /// <summary>
        /// Installs msi from protocol
        /// </summary>
        internal void InstallFromProtocol()
        {
            if (_config is AppConfig config)
            {
                if (string.IsNullOrEmpty(config.Request))
                {
                    _logger.LogWarning("Unable to install from 'msiinstall:// protocol!' Missing Request!");

                    return;
                }

                //drop msiinstall:// protocol prefix
                if (config.Request.StartsWith("msiinstall://"))
                {
                    config.Request = config.Request.Replace("msiinstall://", string.Empty);
                    config.Request = config.Request.Trim('/');
                }

                IMsiUpdate msiUpdate;

                try
                {
                    msiUpdate = JsonConvert.DeserializeObject<MsiUpdate>(Encoding.UTF8.GetString(Convert.FromBase64String(config.Request)));
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unable to install from 'msiinstall:// protocol!' Failed to deserialize '{request}'!", config.Request);

                    return;
                }

                RestartWithAdminPrivileges(msiUpdate);
                Install(msiUpdate);
            }
            else
            {
                _logger.LogWarning("Unable to install from 'msiinstall:// protocol!'");
            }
        }
        #endregion


        #region private methods

        /// <summary>
        /// Stops process specified by update
        /// </summary>
        /// <param name="update">Update that contains information which process should be stopped</param>
        private void StopProcess(IMsiUpdate update)
        {
            if (string.IsNullOrEmpty(update.StopProcessName) || string.IsNullOrEmpty(update.WaitForProcessNameEnd))
            {
                return;
            }

            _logger.LogDebug($"Looking for process with name '{update.StopProcessName}'.");

            Process runningProcess = Process.GetProcesses().SingleOrDefault(process => process.ProcessName == (string.IsNullOrEmpty(update.WaitForProcessNameEnd) ? update.WaitForProcessNameEnd : update.StopProcessName));

            if (runningProcess != null)
            {
                //wait for process to end
                if (string.IsNullOrEmpty(update.WaitForProcessNameEnd))
                {
                    _logger.LogInformation($"Waiting for process end for process'{runningProcess.Id}' with name '{runningProcess.ProcessName}'. Machine: '{{MachineName}}'");

                    runningProcess.WaitForExit(15000);

                    if (runningProcess.HasExited)
                    {
                        return;
                    }
                }

                _logger.LogInformation($"Stopping process '{runningProcess.Id}' with name '{runningProcess.ProcessName}'. Machine: '{{MachineName}}'");

                try
                {
                    runningProcess.Kill();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Failed to stop process '{runningProcess.ProcessName}'!");
                }
            }
        }

        /// <summary>
        /// Restarts application with admin privileges if required
        /// </summary>
        private void RestartWithAdminPrivileges(IMsiUpdate update)
        {
            if (update.AdminPrivilegesRequired.HasValue && update.AdminPrivilegesRequired.Value && IsAdministrator())
            {
                _logger.LogDebug("Restarting with admin privileges. . Machine: '{MachineName}'");

                string[] args = Environment.GetCommandLineArgs();

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = args[0],
                    //UseShellExecute = true,
                    Verb = "runas"
                };

                foreach (string arg in args.Skip(1))
                {
                    psi.ArgumentList.Add(arg);
                }

                Process.Start(psi);
                Environment.Exit(0);
            }
        }
        #endregion
    }
}