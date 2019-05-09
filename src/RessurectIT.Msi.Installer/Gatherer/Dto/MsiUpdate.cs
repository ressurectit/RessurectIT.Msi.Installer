namespace RessurectIT.Msi.Installer.Gatherer.Dto
{
    /// <summary>
    /// Represents data for single update
    /// </summary>
    internal class MsiUpdate
    {
        #region public properties

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
        public string InstallParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets product code that will be used for uninstall previous version
        /// </summary>
        public string UninstallProductCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets parameters used during uninstall
        /// </summary>
        public string UninstallParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name of process to be stopped
        /// </summary>
        public string StopProcessName
        {
            get;
            set;
        }
        #endregion
    }
}