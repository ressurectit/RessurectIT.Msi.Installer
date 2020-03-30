using System;
using System.Timers;
using DryIocAttributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RessurectIT.Msi.Installer.Configuration;
using RessurectIT.Msi.Installer.Gatherer;
using RessurectIT.Msi.Installer.Installer.Dto;
using MsiInstaller = RessurectIT.Msi.Installer.Installer.Installer;

namespace RessurectIT.Msi.Installer.Checker
{
    /// <summary>
    /// Class used for checking availability of updates
    /// </summary>
    [ExportEx]
    internal class UpdateChecker : IDisposable
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
        #endregion


        #region public methods - Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
        #endregion


        #region private methods

        /// <summary>
        /// Performs check of updates
        /// </summary>
        private void DoCheck()
        {
            _logger.LogDebug("Checking for updates! Machine: '{MachineName}'");

            using IServiceScope scope = _serviceProvider.CreateScope();
            
            HttpGatherer gatherer = scope.ServiceProvider.GetService<HttpGatherer>();
            IMsiUpdate[] newUpdates = gatherer.CheckForUpdates();

            MsiInstaller installer = scope.ServiceProvider.GetService<MsiInstaller>();
            installer.Install(newUpdates);
        }
        #endregion
    }
}