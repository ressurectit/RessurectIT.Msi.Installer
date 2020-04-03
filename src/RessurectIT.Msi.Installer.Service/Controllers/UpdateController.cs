using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RessurectIT.Msi.Installer.Gatherer;
using RessurectIT.Msi.Installer.Installer.Dto;
using static RessurectIT.Msi.Installer.Program;

namespace RessurectIT.Msi.Installer.Controllers
{
    /// <summary>
    /// Controller used for obtaining available updates
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UpdateController : ControllerBase
    {
        #region private fields

        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<UpdateController> _logger;

        /// <summary>
        /// Http gatherer used for obtaining available updates
        /// </summary>
        private readonly HttpGatherer _gatherer;

        /// <summary>
        /// Serializer settings used for serialization data to response
        /// </summary>
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        #endregion


        #region constructor

        /// <summary>
        /// Creates instance of <see cref="UpdateController"/>
        /// </summary>
        /// <param name="logger">Logger used for logging</param>
        /// <param name="gatherer">Http gatherer used for obtaining available updates</param>
        public UpdateController(ILogger<UpdateController> logger,
                                HttpGatherer gatherer)
        {
            _logger = logger;
            _gatherer = gatherer;

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
        }
        #endregion


        #region public methods

        /// <summary>
        /// Gets available update for specified id as installation URL
        /// </summary>
        /// <returns>Found <see cref="IMsiUpdate"/> as install msiinstall:// uri or 404</returns>
        [HttpGet("{id}")]
        public ActionResult<string> Get([FromRoute] string id)
        {
            _logger.LogDebug("Getting update for '{id}'. Machine: '{MachineName}'", id);

            IMsiUpdate? result = _gatherer.CheckForUpdates<IMsiUpdate>()
                .Where(update => update.Id == id)
                .OrderByDescending(update => new Version(update.Version))
                .FirstOrDefault();

            if (result != null)
            {
                _logger.LogDebug("Found update for '{id}' is {@update}. Machine: '{MachineName}'", id, result);

                return Content($"msiinstall://{SerializeUpdate(result, _jsonSerializerSettings)}");
            }

            return NotFound();
        }
        #endregion
    }
}
