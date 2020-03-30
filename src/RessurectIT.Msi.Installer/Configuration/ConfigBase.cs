//TODO - add install progress type

namespace RessurectIT.Msi.Installer.Configuration
{
    /// <summary>
    /// Base configuration for application
    /// </summary>
    public class ConfigBase
    {
        #region public properties

        /// <summary>
        /// Gets or sets url that is used for POSTing logs to centralized store
        /// </summary>
        public string? RemoteLogRestUrl
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

        /// <summary>
        /// Gets or sets indication whether is rest logger enabled
        /// </summary>
        public bool RestLoggerEnabled
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


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="ConfigBase"/>
        /// </summary>
        public ConfigBase()
        {
            RestLoggerEnabled = true;
        }
        #endregion
    }
}