using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using RessurectIT.Msi.Installer.Gatherer;
using RessurectIT.Msi.Installer.Gatherer.Dto;
using Serilog;

namespace RessurectIT.Msi.Installer.Checker
{
    /// <summary>
    /// Class used for checking availability of updates
    /// </summary>
    internal class UpdateChecker : IDisposable
    {
        #region private fields

        /// <summary>
        /// Instance of timer used for checking for updates
        /// </summary>
        private readonly Timer _timer = new Timer(RessurectITMsiInstallerService.Config.CheckInterval);

        /// <summary>
        /// Instance of http gatherer used for gathering info about available updates
        /// </summary>
        private readonly HttpGatherer _gatherer = new HttpGatherer();
        #endregion


        #region public methods

        /// <summary>
        /// Starts update checker that checks for updates
        /// </summary>
        public void Start()
        {
            _timer.Elapsed += DoCheck;
            _timer.AutoReset = true;

            DoCheck(null, null);
            _timer.Start();
        }
        #endregion


        #region public methods - Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _gatherer.Dispose();
            _timer.Stop();
            _timer.Dispose();
        }
        #endregion


        #region private methods

        /// <summary>
        /// Performs check of updates
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event object</param>
        private void DoCheck(object sender, ElapsedEventArgs e)
        {
            Log.Information("Checking for updates! Machine: '{MachineName}'");

            MsiUpdate[] newUpdates = _gatherer.CheckForUpdates();

            foreach (MsiUpdate update in newUpdates)
            {
                Log.Information($"Installing update for {update.Id}, version {update.Version}. Machine: '{{MachineName}}'");

                Installer.WindowsInstaller installer = new Installer.WindowsInstaller(update);

                try
                {
                    StopProcess(update);
                    installer.Uninstall();
                    installer.Install();
                }
                catch (InstallException ex)
                {
                    Log.Error(ex, "Failed to install new update! Machine: '{MachineName}'");

                    continue;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to install new update! Machine: '{MachineName}'");

                    continue;
                }

                update.UninstallProductCode = Installer.WindowsInstaller.GetProductCode(update.MsiPath);

                _gatherer.SetInstalledUpdates(update);

                Log.Information($"Installation of update for '{update.Id}' version '{update.Version}' was successful. Machine: '{{MachineName}}'");
            }
        }

        /// <summary>
        /// Stops process specified by update
        /// </summary>
        /// <param name="update">Update that contains information which process should be stopped</param>
        private void StopProcess(MsiUpdate update)
        {
            if (string.IsNullOrEmpty(update.StopProcessName))
            {
                return;
            }

            Log.Information($"Looking for process with name '{update.StopProcessName}'.");

            Process runningProcess = Process.GetProcesses().SingleOrDefault(process => process.ProcessName == update.StopProcessName);

            if (runningProcess != null)
            {
                Log.Information($"Stopping process '{runningProcess.Id}' with name '{runningProcess.ProcessName}'.");

                try
                {
                    runningProcess.Kill();
                }
                catch (Exception e)
                {
                    Log.Warning(e, $"Failed to stop process '{runningProcess.ProcessName}'!");
                }
            }
        }
        #endregion
    }
}