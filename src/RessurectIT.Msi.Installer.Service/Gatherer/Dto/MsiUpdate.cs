﻿using RessurectIT.Msi.Installer.Installer.Dto;

namespace RessurectIT.Msi.Installer.Gatherer.Dto
{
    /// <summary>
    /// Represents data for single update
    /// </summary>
    internal class MsiUpdate : IMsiUpdate
    {
        #region public properties

        /// <inheritdoc />
        public string Version
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

        /// <summary>
        /// Gets or sets download url for MSI
        /// </summary>
        public string MsiDownloadUrl
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