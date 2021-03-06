﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using DryIocAttributes;
using Microsoft.Extensions.Logging;
using RessurectIT.Msi.Installer.Gatherer.Dto;
using Newtonsoft.Json;
using RessurectIT.Msi.Installer.Configuration;
using RessurectIT.Msi.Installer.Installer;
using RessurectIT.Msi.Installer.Installer.Dto;
using RessurectIT.Msi.Installer.UpdatesDatabase;
using RessurectIT.Msi.Installer.UpdatesDatabase.Dto;

//TODO - primitive way of caching is done, improve it with storing hash of MSI

namespace RessurectIT.Msi.Installer.Gatherer
{
    /// <summary>
    /// Class used for gathering information about available updates
    /// </summary>
    [ExportEx]
    [CurrentScopeReuse]
    public class HttpGatherer : IDisposable
    {
        #region private fields
       
        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<HttpGatherer> _logger;

        /// <summary>
        /// Service configuration instance
        /// </summary>
        private readonly ServiceConfig _config;

        /// <summary>
        /// Service used for managing installed updates
        /// </summary>
        private readonly IUpdatesDatabase _updatesDatabase;

        /// <summary>
        /// Http client used for calling rest services
        /// </summary>
        private readonly HttpClient _httpClient;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="HttpGatherer"/>
        /// </summary>
        /// <param name="logger">Logger used for logging</param>
        /// <param name="config">Service configuration instance</param>
        /// <param name="updatesDatabase">Service used for managing installed updates</param>
        public HttpGatherer(ILogger<HttpGatherer> logger,
                            ServiceConfig config,
                            IUpdatesDatabase updatesDatabase)
        {
            _logger = logger;
            _config = config;
            _updatesDatabase = updatesDatabase;
            _httpClient = new HttpClient();
        }
        #endregion


        #region public methods

        /// <summary>
        /// Checks for updates and returns array of updates to install
        /// </summary>
        /// <returns>Array of updates that should be installed</returns>
        public TUpdate[] CheckForUpdates<TUpdate>() where TUpdate : IMsiUpdate
        {
            HttpResponseMessage result;

            try
            {
                result = _httpClient.GetAsync(_config.UpdatesJsonUrl).Result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to obtain updates json! Machine: '{MachineName}'");

                return new TUpdate[0];
            }

            Dictionary<string, MsiUpdate> updates;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    string body = result.Content.ReadAsStringAsync().Result;

                    _logger.LogDebug("Available updates: '{body}'", body);

                    updates = JsonConvert.DeserializeObject<Dictionary<string, MsiUpdate>>(body) ?? new Dictionary<string, MsiUpdate>();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error during deserialization of updates result. Machine: '{MachineName}'");

                    return new TUpdate[0];
                }
            }
            else
            {
                _logger.LogError($"Obtaining failed, returned status code '{result.StatusCode}'. Machine: '{{MachineName}}'");

                return new TUpdate[0];
            }

            Dictionary<string, InstalledUpdateInfo> installedUpdates = _updatesDatabase.GetInstalledUpdates();
            MsiUpdate[] newUpdates = updates
                .Keys
                .Select(updateId =>
                {
                    MsiUpdate newUpdate = updates[updateId];

                    newUpdate.Id = updateId;

                    return newUpdate;
                })
                .Where(update =>
                {
                    if (string.IsNullOrEmpty(update.MsiDownloadUrl))
                    {
                        _logger.LogError("Update is missing MSI download URL! Machine: '{MachineName}'");

                        return false;
                    }

                    try
                    {
                        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(update.MsiDownloadUrl));

                        //local msi does not exists
                        if (!File.Exists(tempPath))
                        {
                            HttpResponseMessage msiResult = _httpClient.GetAsync(update.MsiDownloadUrl).Result;

                            using (Stream contentStream = msiResult.Content.ReadAsStreamAsync().Result)
                            using (Stream fileStream = File.Create(tempPath))
                            {
                                contentStream.CopyTo(fileStream);
                            }
                        }

                        update.MsiPath = tempPath;
                        update.Version = WindowsInstaller.GetMsiVersion(tempPath);
                        update.ComputedHash = ComputeHash(tempPath)!;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, $"Unable to obtain msi for '{update.Id}' with url '{update.MsiDownloadUrl}'!");

                        return false;
                    }

                    return true;
                }).ToArray();

            _logger.LogDebug("Preprocessed new updates {@newUpdates}", (object)newUpdates);

            return (from update in newUpdates
                    join installedUpdateIdJoin in installedUpdates.Keys on update.Id equals installedUpdateIdJoin into installedUpdatesIds
                    from installedUpdateId in installedUpdatesIds.DefaultIfEmpty()
                    let updateVersion = new Version(update.Version)
                    let installedVersion = installedUpdateId != null ? installedUpdates[installedUpdateId].GetVersionObj(_logger) : null
                    where !string.IsNullOrEmpty(update.MsiPath) &&
                          ((installedUpdateId == null || installedVersion < updateVersion) ||
                           (_config.AllowSameVersion && installedVersion == updateVersion && update.ComputedHash != installedUpdates[installedUpdateId].Hash))
                    select (TUpdate) (IMsiUpdate) new MsiUpdate
                    {
                        Id = update.Id,
                        Version = update.Version,
                        ComputedHash = update.ComputedHash,
                        InstallParameters = update.InstallParameters,
                        MsiDownloadUrl = update.MsiDownloadUrl,
                        MsiPath = update.MsiPath,
                        StopProcessName = update.StopProcessName,
                        UninstallParameters = update.UninstallParameters ?? (installedUpdateId != null ? installedUpdates[installedUpdateId].InstallParameters : null),
                        UninstallProductCode = update.UninstallProductCode ?? (installedUpdateId != null ? installedUpdates[installedUpdateId].ProductCode : null),
                        WaitForProcessNameEnd = update.WaitForProcessNameEnd,
                        AutoInstall = update.AutoInstall,
                        AdminPrivilegesRequired = update.AdminPrivilegesRequired,
                        StartProcessPath = update.StartProcessPath,
                        ForceStop = update.ForceStop,
                        Notify = update.Notify
                    }).ToArray();
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
        /// Computes hash of msi file
        /// </summary>
        /// <param name="msiPath">Path to msi file</param>
        /// <returns>String hash of msi file</returns>
        private string? ComputeHash(string msiPath)
        {
            if (string.IsNullOrEmpty(msiPath) || !File.Exists(msiPath))
            {
                return null;
            }

            using SHA1 sha1 = SHA1.Create();
            using Stream stream = File.OpenRead(msiPath);

            byte[] hash = sha1.ComputeHash(stream);

            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        #endregion
    }
}