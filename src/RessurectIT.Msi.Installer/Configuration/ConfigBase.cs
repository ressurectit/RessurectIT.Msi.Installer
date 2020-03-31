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

        /// <summary>
        /// Gets or sets setting for displaying install progress
        /// </summary>
        public string? Progress
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


        #region internal properties

        /// <summary>
        /// Gets setting for displaying install progress
        /// </summary>
        internal ProgressType ProgressType
        {
            get
            {
                if (string.IsNullOrEmpty(Progress))
                {
                    return ProgressType.MsiExec;
                }

                return Progress switch
                {
                    nameof(ProgressType.None) => ProgressType.None,
                    nameof(ProgressType.App) => ProgressType.App,
                    _ => ProgressType.MsiExec
                };
            }
        }
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