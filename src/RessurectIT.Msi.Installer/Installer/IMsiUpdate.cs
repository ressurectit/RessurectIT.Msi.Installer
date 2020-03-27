namespace RessurectIT.Msi.Installer.Installer
{
    /// <summary>
    /// Object describing msi update
    /// </summary>
    public interface IMsiUpdate
    {
        #region properties

        /// <summary>
        /// Gets or sets version of msi
        /// </summary>
        string Version
        {
            get;
        }

        /// <summary>
        /// Path to msi download to local computer
        /// </summary>
        string MsiPath
        {
            get;
        }

        /// <summary>
        /// Gets or sets parameters used for installation of msi
        /// </summary>
        string? InstallParameters
        {
            get;
        }

        /// <summary>
        /// Gets or sets product code that will be used for uninstall previous version
        /// </summary>
        string? UninstallProductCode
        {
            get;
        }

        /// <summary>
        /// Gets or sets parameters used during uninstall
        /// </summary>
        string? UninstallParameters
        {
            get;
        }
        #endregion
    }
}