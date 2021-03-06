﻿namespace RessurectIT.Msi.Installer.Configuration
{
    /// <summary>
    /// Configuration for service
    /// </summary>
    public class ServiceConfig : ConfigBase
    {
        #region public properties

        /// <summary>
        /// Gets or sets URL used for obtaining json
        /// </summary>
        public string UpdatesJsonUrl
        {
            get;
            set;
        } = "http://localhost:8888/updates.json";

        /// <summary>
        /// Gets or sets indication whether is same version allowed to be reinstalled
        /// </summary>
        public bool AllowSameVersion
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
        } = 300000;

        /// <summary>
        /// Gets or sets indication whether to automatically install update when discovered
        /// </summary>
        public bool AutoInstall
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets indication whether run service as local not windows service
        /// </summary>
        public bool LocalServer
        {
            get;
            set;
        }
        #endregion
    }
}