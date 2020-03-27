using DryIocAttributes;
using Microsoft.Extensions.Logging;
using RessurectIT.Msi.Installer.Configuration;

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
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="Installer"/>
        /// </summary>
        /// <param name="config">Application configuration</param>
        /// <param name="logger">Logger used for logging</param>
        public Installer(ConfigBase config, ILogger<Installer> logger)
        {
            _config = config;
            _logger = logger;
        }
        #endregion


        #region public methods

        public void Install()
        {
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
            }
            else
            {
                _logger.LogWarning("Unable to install from 'msiinstall:// protocol!'");
            }
        }
        #endregion
    }
}