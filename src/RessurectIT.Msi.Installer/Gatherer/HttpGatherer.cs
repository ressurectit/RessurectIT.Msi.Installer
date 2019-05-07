using System.Net.Http;
using RessurectIT.Msi.Installer.Gatherer.Dto;
using Newtonsoft.Json;

namespace RessurectIT.Msi.Installer.Gatherer
{
    /// <summary>
    /// Class used for gathering information about available updates
    /// </summary>
    internal class HttpGatherer
    {
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

            //HttpResponseMessage result = _httpClient.GetAsync(RessurectITMsiInstallerService.Config.UpdatesJsonUrl).Result;
            HttpResponseMessage result = _httpClient.GetAsync("http://localhost:8888/updates.json").Result;

            MsiInstallerUpdates updates = JsonConvert.DeserializeObject<MsiInstallerUpdates>(result.Content.ReadAsStringAsync().Result);
        }
        #endregion


        #region public methods


        #endregion
    }
}