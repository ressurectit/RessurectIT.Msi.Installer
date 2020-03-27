namespace RessurectIT.Msi.Installer.Gatherer.Dto
{
    /// <summary>
    /// Represents object received that holds information about all available updates
    /// </summary>
    internal class MsiInstallerUpdates
    {
        #region public properties

        /// <summary>
        /// Gets or sets array of available updates
        /// </summary>
        public MsiUpdate[] Updates
        {
            get;
            set;
        }
        #endregion
    }
}