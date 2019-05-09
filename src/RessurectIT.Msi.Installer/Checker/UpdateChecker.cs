using System;
using System.Timers;
using RessurectIT.Msi.Installer.Gatherer;
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
            Log.Information("Checking for updates!");

            _gatherer.CheckForUpdates();
        }
        #endregion
    }
}