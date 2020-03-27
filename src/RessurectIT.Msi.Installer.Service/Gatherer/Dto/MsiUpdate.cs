using RessurectIT.Msi.Installer.Installer;

namespace RessurectIT.Msi.Installer.Gatherer.Dto
{
    /// <summary>
    /// Represents data for single update
    /// </summary>
    public class MsiUpdate : IMsiUpdate
    {
        #region public properties

        /// <summary>
        /// Gets or sets version of msi
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Path to msi download to local computer
        /// </summary>
        public string MsiPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets computed hash of msi file
        /// </summary>
        public string ComputedHash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets unique id of application to be upgraded, should be same for all versions of application
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets download url for MSI
        /// </summary>
        public string MsiDownloadUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets parameters used for installation of msi
        /// </summary>
        public string? InstallParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets product code that will be used for uninstall previous version
        /// </summary>
        public string? UninstallProductCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets parameters used during uninstall
        /// </summary>
        public string? UninstallParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name of process to be stopped
        /// </summary>
        public string? StopProcessName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name of process to be waited for
        /// </summary>
        public string? WaitForProcessNameEnd
        {
            get;
            set;
        }
        #endregion
    }
}