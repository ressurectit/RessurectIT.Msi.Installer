using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RessurectIT.Msi.Installer.Checker;
using RessurectIT.Msi.Installer.Configuration;

//TODO - changed auto install way of working

namespace RessurectIT.Msi.Installer.Services
{
    /// <summary>
    /// Update checker as hosted service
    /// </summary>
    public class UpdateCheckerHostService : IHostedService, IDisposable
    {
        #region private fields

        /// <summary>
        /// Service used for checking of new updates
        /// </summary>
        private readonly UpdateChecker _updateChecker;

        /// <summary>
        /// Service configuration
        /// </summary>
        private readonly ServiceConfig _config;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="UpdateCheckerHostService"/>
        /// </summary>
        /// <param name="updateChecker">Service used for checking of new updates</param>
        /// <param name="config">Service configuration</param>
        public UpdateCheckerHostService(UpdateChecker updateChecker,
                                        ServiceConfig config)
        {
            _updateChecker = updateChecker;
            _config = config;
        }
        #endregion


        #region public methods - Implementation of IHostedService

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_config.AutoInstall)
            {
                _updateChecker.Start();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_config.AutoInstall)
            {
                _updateChecker.Stop();
            }

            return Task.CompletedTask;
        }
        #endregion


        #region public methods - Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _updateChecker.Dispose();
        }
        #endregion
    }
}