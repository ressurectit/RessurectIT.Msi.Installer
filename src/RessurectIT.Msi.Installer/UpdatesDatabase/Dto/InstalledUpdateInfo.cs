using System;
using Microsoft.Extensions.Logging;

namespace RessurectIT.Msi.Installer.UpdatesDatabase.Dto
{
    /// <summary>
    /// Information about installed update
    /// </summary>
    public class InstalledUpdateInfo
    {
        #region public properties

        /// <summary>
        /// Gets or sets last installed version
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets product code of last installed version
        /// </summary>
        public string ProductCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets hash of installed msi file
        /// </summary>
        public string Hash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets parameters used for installation of msi, later used for uninstalling of msi
        /// </summary>
        public string? InstallParameters
        {
            get;
            set;
        }
        #endregion


        #region public methods

        /// <summary>
        /// Gets version as object representing version
        /// </summary>
        public Version GetVersionObj<TLoggerType>(ILogger<TLoggerType> logger)
        {
            try
            {
                return new Version(Version);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Version '{Version}' for '{ProductCode}' is in incorrect format!");

                return new Version("0.0.0.0");
            }
        }
        #endregion
    }
}