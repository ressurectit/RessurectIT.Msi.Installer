namespace RessurectIT.Msi.Installer.Gatherer.Dto
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
        #endregion
    }
}