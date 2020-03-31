using System;
using System.Collections.Generic;
using System.IO;
using DryIocAttributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RessurectIT.Msi.Installer.Installer.Dto;
using RessurectIT.Msi.Installer.UpdatesDatabase.Dto;

namespace RessurectIT.Msi.Installer.UpdatesDatabase
{
    /// <summary>
    /// Used for managing installed updates database
    /// </summary>
    [ExportEx(typeof(IUpdatesDatabase))]
    internal class UpdatesDatabase : IUpdatesDatabase
    {
        #region constants

        /// <summary>
        /// Name of file that is used for storing information about installed updates
        /// </summary>
        private const string InstalledUpdatesJson = "installed.updates.json";
        #endregion


        #region private fields

        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<UpdatesDatabase> _logger;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="UpdatesDatabase"/>
        /// </summary>
        /// <param name="logger">Logger used for logging</param>
        public UpdatesDatabase(ILogger<UpdatesDatabase> logger)
        {
            _logger = logger;
        }
        #endregion


        #region public methods

        /// <inheritdoc />
        public Dictionary<string, InstalledUpdateInfo> GetInstalledUpdates()
        {
            Dictionary<string, InstalledUpdateInfo> installedUpdates = new Dictionary<string, InstalledUpdateInfo>();

            if (File.Exists(InstalledUpdatesJson))
            {
                try
                {
                    installedUpdates = JsonConvert.DeserializeObject<Dictionary<string, InstalledUpdateInfo>>(File.ReadAllText(InstalledUpdatesJson));
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to load installed updates!");
                }
            }

            return installedUpdates;
        }

        /// <summary>
        /// Sets installed update to stored json
        /// </summary>
        /// <param name="update">Information about installed update</param>
        public void SetInstalledUpdates(IMsiUpdate update)
        {
            Dictionary<string, InstalledUpdateInfo> installedUpdates = GetInstalledUpdates();

            installedUpdates[update.Id] = new InstalledUpdateInfo
            {
                Version = update.Version,
                ProductCode = update.UninstallProductCode!,
                Hash = update.ComputedHash,
                InstallParameters = update.InstallParameters
            };

            try
            {
                File.WriteAllText(InstalledUpdatesJson, JsonConvert.SerializeObject(installedUpdates, Formatting.Indented));
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to store installed updates json!");
            }
        }
        #endregion
    }
}