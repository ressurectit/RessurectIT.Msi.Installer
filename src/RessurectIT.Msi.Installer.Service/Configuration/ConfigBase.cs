namespace RessurectIT.Msi.Installer.Configuration
{
    /// <summary>
    /// Configuration for service
    /// </summary>
    internal class ServiceConfig : ConfigBase
    {
        #region public properties

        /// <summary>
        /// Gets or sets URL used for obtaining json
        /// </summary>
        public string UpdatesJsonUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets interval when to check for updates in ms
        /// </summary>
        public int CheckInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets indication whether to automatically install update when discovered
        /// </summary>
        public bool AutoInstall
        {
            get;
            set;
        }
        #endregion
    }
}