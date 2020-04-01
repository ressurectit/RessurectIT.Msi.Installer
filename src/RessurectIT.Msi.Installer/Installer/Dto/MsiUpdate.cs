namespace RessurectIT.Msi.Installer.Installer.Dto
{
    /// <summary>
    /// Msi update object
    /// </summary>
    internal class MsiUpdate : IMsiUpdate
    {
        #region public properties - Implementation of IMsiUpdate

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "CS8618:Non-nullable property is uninitialized. Consider declaring as nullable.", Justification = "<Pending>")]
        public string Version
        {
            get;
            set;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "CS8618:Non-nullable property is uninitialized. Consider declaring as nullable.", Justification = "<Pending>")]
        public string MsiPath
        {
            get;
            set;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "CS8618:Non-nullable property is uninitialized. Consider declaring as nullable.", Justification = "<Pending>")]
        public string ComputedHash
        {
            get;
            set;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "CS8618:Non-nullable property is uninitialized. Consider declaring as nullable.", Justification = "<Pending>")]
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
        public bool? ForceStop
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

        /// <inheritdoc />
        public string? StartProcessPath
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool? AdminPrivilegesRequired
        {
            get;
            set;
        }
        #endregion
    }
}