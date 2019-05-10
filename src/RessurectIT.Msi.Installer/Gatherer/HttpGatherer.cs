using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using RessurectIT.Msi.Installer.Gatherer.Dto;
using Newtonsoft.Json;
using Serilog;

namespace RessurectIT.Msi.Installer.Gatherer
{
    /// <summary>
    /// Class used for gathering information about available updates
    /// </summary>
    internal class HttpGatherer : IDisposable
    {
        #region constants

        /// <summary>
        /// Name of file that is used for storing information about installed updates
        /// </summary>
        private const string InstalledUpdatesJson = "installed.updates.json";
        #endregion


        #region private fields
       
        /// <summary>
        /// Http client used for calling rest services
        /// </summary>
        private readonly HttpClient _httpClient;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="HttpGatherer"/>
        /// </summary>
        public HttpGatherer()
        {
            _httpClient = new HttpClient();
        }
        #endregion


        #region public methods

        /// <summary>
        /// Checks for updates and returns array of updates to install
        /// </summary>
        /// <returns>Array of updates that should be installed</returns>
        public MsiUpdate[] CheckForUpdates()
        {
            HttpResponseMessage result = _httpClient.GetAsync(RessurectITMsiInstallerService.Config.UpdatesJsonUrl).Result;
            MsiInstallerUpdates updates;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    updates = JsonConvert.DeserializeObject<MsiInstallerUpdates>(result.Content.ReadAsStringAsync().Result);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error during deserialization of updates result. Machine: '{MachineName}'");

                    return new MsiUpdate[0];
                }
            }
            else
            {
                Log.Error($"Obtaining failed, returned status code '{result.StatusCode}'. Machine: '{{MachineName}}'");

                return new MsiUpdate[0];
            }

            updates.Updates = updates.Updates.Where(update =>
            {
                if (string.IsNullOrEmpty(update.Id))
                {
                    Log.Error("Update is missing ID! Machine: '{MachineName}'");

                    return false;
                }

                try
                {
                    HttpResponseMessage msiResult = _httpClient.GetAsync(update.MsiDownloadUrl).Result;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(update.MsiDownloadUrl));

                    using (Stream contentStream = msiResult.Content.ReadAsStreamAsync().Result)
                    using (Stream fileStream = File.Create(tempPath))
                    {
                        contentStream.CopyTo(fileStream);
                    }

                    update.MsiPath = tempPath;
                    update.Version = Installer.WindowsInstaller.GetMsiVersion(tempPath);
                }
                catch (Exception e)
                {
                    Log.Warning(e, $"Unable to obtain msi for '{update.Id}' with url '{update.MsiDownloadUrl}'!");

                    return false;
                }

                return true;
            }).ToArray();

            Dictionary<string, InstalledUpdateInfo> installedUpdates = GetInstalledUpdates();

            return (from update in updates.Updates
                    join installedUpdateIdJoin in installedUpdates.Keys on update.Id equals installedUpdateIdJoin into installedUpdatesIds
                    from installedUpdateId in installedUpdatesIds.DefaultIfEmpty()
                    where !string.IsNullOrEmpty(update.MsiPath) && (installedUpdateId == null || installedUpdates[installedUpdateId].VersionObj < new Version(update.Version))
                    select new MsiUpdate
                    {
                        Id = update.Id,
                        Version = update.Version,
                        InstallParameters = update.InstallParameters,
                        MsiDownloadUrl = update.MsiDownloadUrl,
                        MsiPath = update.MsiPath,
                        StopProcessName = update.StopProcessName,
                        UninstallParameters = update.UninstallParameters,
                        UninstallProductCode = update.UninstallProductCode ?? (installedUpdateId != null ? installedUpdates[installedUpdateId].ProductCode : null)
                    }).ToArray();
        }

        /// <summary>
        /// Sets installed update to stored json
        /// </summary>
        /// <param name="update">Information about installed update</param>
        public void SetInstalledUpdates(MsiUpdate update)
        {
            Dictionary<string, InstalledUpdateInfo> installedUpdates = GetInstalledUpdates();

            installedUpdates[update.Id] = new InstalledUpdateInfo
            {
                Version = update.Version,
                ProductCode = update.UninstallProductCode
            };

            try
            {
                File.WriteAllText(InstalledUpdatesJson, JsonConvert.SerializeObject(installedUpdates, Formatting.Indented));
            }
            catch (Exception e)
            {
                Log.Warning(e, "Failed to store installed updates json!");
            }
        }
        #endregion


        #region public methods - Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient.Dispose();
        }
        #endregion


        #region private methods

        /// <summary>
        /// Gets installed updates object
        /// </summary>
        /// <returns>Gets information about installed updates</returns>
        private Dictionary<string, InstalledUpdateInfo> GetInstalledUpdates()
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
                    Log.Warning(e, "Failed to load installed updates!");
                }
            }

            return installedUpdates;
        }
        #endregion
    }
}