using System;
using System.IO;
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
        /// 
        /// </summary>
        /// <returns></returns>
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
                    Log.Error(e, "Error during deserialization of updates result.");

                    return new MsiUpdate[0];
                }
            }
            else
            {
                Log.Error($"Obtaining failed, returned status code '{result.StatusCode}'.");

                return new MsiUpdate[0];
            }

            if (File.Exists(InstalledUpdatesJson))
            {
                try
                {
                    File.ReadAllLines(InstalledUpdatesJson);
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }
        #endregion


        #region public methods - Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient.Dispose();
        }
        #endregion
    }
}