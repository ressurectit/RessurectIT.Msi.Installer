using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using RessurectIT.Msi.Installer.Gatherer.Dto;
using Newtonsoft.Json;
using RessurectIT.Msi.Installer.UpdatesDatabase.Dto;

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
            HttpResponseMessage result;

            try
            {
                result = _httpClient.GetAsync("").Result;
                //result = _httpClient.GetAsync(RessurectITMsiInstallerService.Config.UpdatesJsonUrl).Result;
            }
            catch (Exception e)
            {
                //Log.Error(e, "Unable to obtain updates json! Machine: '{MachineName}'");

                return new MsiUpdate[0];
            }

            MsiInstallerUpdates updates;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    updates = JsonConvert.DeserializeObject<MsiInstallerUpdates>(result.Content.ReadAsStringAsync().Result);
                }
                catch (Exception e)
                {
                    //Log.Error(e, "Error during deserialization of updates result. Machine: '{MachineName}'");

                    return new MsiUpdate[0];
                }
            }
            else
            {
                //Log.Error($"Obtaining failed, returned status code '{result.StatusCode}'. Machine: '{{MachineName}}'");

                return new MsiUpdate[0];
            }

            updates.Updates = updates.Updates.Where(update =>
            {
                if (string.IsNullOrEmpty(update.Id))
                {
                    //Log.Error("Update is missing ID! Machine: '{MachineName}'");

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
                    update.ComputedHash = ComputeHash(tempPath);
                }
                catch (Exception e)
                {
                    //Log.Warning(e, $"Unable to obtain msi for '{update.Id}' with url '{update.MsiDownloadUrl}'!");

                    return false;
                }

                return true;
            }).ToArray();

            Dictionary<string, InstalledUpdateInfo> installedUpdates = GetInstalledUpdates();

            return (from update in updates.Updates
                    join installedUpdateIdJoin in installedUpdates.Keys on update.Id equals installedUpdateIdJoin into installedUpdatesIds
                    from installedUpdateId in installedUpdatesIds.DefaultIfEmpty()
                    let updateVersion = new Version(update.Version)
                    where !string.IsNullOrEmpty(update.MsiPath) && 
                          (installedUpdateId == null || installedUpdates[installedUpdateId].VersionObj < updateVersion) ||
                          (false && installedUpdates[installedUpdateId].VersionObj == updateVersion && update.ComputedHash != installedUpdates[installedUpdateId].Hash)
                          //(RessurectITMsiInstallerService.Config.AllowSameVersion && installedUpdates[installedUpdateId].VersionObj == updateVersion && update.ComputedHash != installedUpdates[installedUpdateId].Hash)
                    select new MsiUpdate
                    {
                        Id = update.Id,
                        Version = update.Version,
                        ComputedHash = update.ComputedHash,
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
                ProductCode = update.UninstallProductCode,
                Hash = update.ComputedHash
            };

            try
            {
                File.WriteAllText(InstalledUpdatesJson, JsonConvert.SerializeObject(installedUpdates, Formatting.Indented));
            }
            catch (Exception e)
            {
                //Log.Warning(e, "Failed to store installed updates json!");
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
                    //Log.Warning(e, "Failed to load installed updates!");
                }
            }

            return installedUpdates;
        }

        /// <summary>
        /// Computes hash of msi file
        /// </summary>
        /// <param name="msiPath">Path to msi file</param>
        /// <returns>String hash of msi file</returns>
        private string ComputeHash(string msiPath)
        {
            if (string.IsNullOrEmpty(msiPath) || !File.Exists(msiPath))
            {
                return null;
            }

            using (SHA1 sha1 = SHA1.Create())
            {
                using (Stream stream = File.OpenRead(msiPath))
                {
                    byte[] hash = sha1.ComputeHash(stream);

                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        #endregion
    }
}