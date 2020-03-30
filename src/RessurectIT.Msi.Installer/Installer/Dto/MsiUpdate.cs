namespace RessurectIT.Msi.Installer.Installer.Dto
{
    /// <summary>
    /// Msi update object
    /// </summary>
    internal class MsiUpdate : IMsiUpdate
    {
        #region public properties - Implementation of IMsiUpdate

        /// <inheritdoc />
        public string? Version
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string MsiPath
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string ComputedHash
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string Id
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string? InstallParameters
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string? UninstallProductCode
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string? UninstallParameters
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string? StopProcessName
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string? WaitForProcessNameEnd
        {
            get;
            set;
        }
        #endregion
    }
}