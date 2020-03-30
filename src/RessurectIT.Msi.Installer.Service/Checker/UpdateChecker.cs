using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using RessurectIT.Msi.Installer.Gatherer;
using RessurectIT.Msi.Installer.Gatherer.Dto;

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
        private readonly Timer _timer = new Timer(10000);
        //private readonly Timer _timer = new Timer(RessurectITMsiInstallerService.Config.CheckInterval);

        /// <summary>
        /// Instance of http gatherer used for gathering info about available updates
        /// </summary>
        private readonly HttpGatherer _gatherer = new HttpGatherer();

        /// <summary>
        /// Callback called when there is need to stop installer itself
        /// </summary>
        private readonly Action _stopCallback;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="UpdateChecker"/>
        /// </summary>
        /// <param name="stopCallback">Callback called when there is need to stop installer itself</param>
        public UpdateChecker(Action stopCallback)
        {
            _stopCallback = stopCallback;
        }
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
            //Log.Information("Checking for updates! Machine: '{MachineName}'");

            MsiUpdate[] newUpdates = _gatherer.CheckForUpdates();

            foreach (MsiUpdate update in newUpdates)
            {
                bool @break = false;

                //Log.Information($"Installing update for {update.Id}, version {update.Version}, from {Path.GetFileName(update.MsiPath)}. Machine: '{{MachineName}}'");

                Installer.WindowsInstaller installer = new Installer.WindowsInstaller(update,
                                                                                      () =>
                                                                                      {
                                                                                          @break = true;
                                                                                          _stopCallback();
                                                                                      });

                try
                {
                    StopProcess(update);
                    installer.Uninstall();
                    installer.Install();

                    if (@break)
                    {
                        break;
                    }
                }
                //catch (InstallationException ex)
                //{
                //    Log.Error(ex, "Failed to install new update! Machine: '{MachineName}'");

                //    continue;
                //}
                catch (Exception ex)
                {
                    //Log.Error(ex, "Failed to install new update! Machine: '{MachineName}'");

                    continue;
                }

                update.UninstallProductCode = Installer.WindowsInstaller.GetProductCode(update.MsiPath);

                _gatherer.SetInstalledUpdates(update);

                //Log.Information($"Installation of update for '{update.Id}' version '{update.Version}' was successful. Machine: '{{MachineName}}'");
            }
        }
        #endregion
    }
}