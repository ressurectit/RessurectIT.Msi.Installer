namespace RessurectIT.Msi.Installer.Installer.Dto
{
    /// <summary>
    /// Object describing single msi update
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
        /// Gets or sets computed hash of msi file
        /// </summary>
        string ComputedHash
        {
            get;
        }

        /// <summary>
        /// Gets or sets unique id of application to be upgraded, should be same for all versions of application
        /// </summary>
        string Id
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
            set;
        }

        /// <summary>
        /// Gets or sets parameters used during uninstall
        /// </summary>
        string? UninstallParameters
        {
            get;
        }

        /// <summary>
        /// Gets or sets name of process to be stopped
        /// </summary>
        string? StopProcessName
        {
            get;
        }

        /// <summary>
        /// Gets or sets name of process to be waited for
        /// </summary>
        string? WaitForProcessNameEnd
        {
            get;
        }

        /// <summary>
        /// Gets or sets path to process which should be start after installation
        /// </summary>
        string? StartProcessPath
        {
            get;
        }

        /// <summary>
        /// Gets or sets indication whether are admin privileges required
        /// </summary>
        bool? AdminPrivilegesRequired
        {
            get;
        }
        #endregion
    }
}