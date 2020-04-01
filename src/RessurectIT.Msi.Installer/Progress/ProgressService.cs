using System;
using System.Threading.Tasks;
using DryIocAttributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RessurectIT.Msi.Installer.Configuration;

namespace RessurectIT.Msi.Installer.Progress
{
    /// <summary>
    /// Used for displaying progress indicator
    /// </summary>
    [ExportEx(typeof(IProgressService))]
    [CurrentScopeReuse]
    internal class ProgressService : IProgressService
    {
        #region private fields

        /// <summary>
        /// Service provider used for obtaining <see cref="IProgressWindow"/>
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<ProgressService> _logger;

        /// <summary>
        /// Application configuration
        /// </summary>
        private readonly ConfigBase _config;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="ProgressService"/>
        /// </summary>
        /// <param name="serviceProvider">Service provider used for obtaining <see cref="IProgressWindow"/></param>
        /// <param name="logger">Logger used for logging</param>
        /// <param name="config">Application configuration</param>
        public ProgressService(IServiceProvider serviceProvider,
                               ILogger<ProgressService> logger, 
                               ConfigBase config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }
        #endregion


        #region public methods - Implementation of IProgressService

        /// <inheritdoc />
        public async Task ShowProgressMessage(string message, string updateId)
        {
            if (!Environment.UserInteractive)
            {
                _logger.LogDebug("Running without user interactive mode.");

                return;
            }

            //do not display this type of progress
            if (_config.ProgressType != ProgressType.App)
            {
                return;
            }

            IProgressWindow progressWindow = _serviceProvider.GetService<IProgressWindow>();
            progressWindow.ShowProgressMessage(message, updateId);

            //forcing switching of threads
            await Task.Delay(3);
        }
        #endregion
    }
}