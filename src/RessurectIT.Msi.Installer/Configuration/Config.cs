namespace RessurectIT.Msi.Installer.Configuration
{
    /// <summary>
    /// Configuration for application
    /// </summary>
    public class Config
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
        /// Gets or sets url that is used for POSTing logs to centralized store
        /// </summary>
        public string RemoteLogRestUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets indication whether is same version allowed to be reinstalled
        /// </summary>
        public bool AllowSameVersion
        {
            get;
            set;
        }

#if DEBUG
        /// <summary>
        /// Gets or sets indication
        /// </summary>
        public bool Debugging
        {
            get;
            set;
        }
#endif
        #endregion
    }
}