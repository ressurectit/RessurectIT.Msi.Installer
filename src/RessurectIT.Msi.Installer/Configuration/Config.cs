namespace RessurectIT.Msi.Installer.Configuration
{
    /// <summary>
    /// Configuration for application
    /// </summary>
    internal class Config
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
        #endregion
    }
}