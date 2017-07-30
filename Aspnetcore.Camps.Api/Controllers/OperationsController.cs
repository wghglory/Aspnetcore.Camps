using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aspnetcore.Camps.Api.Controllers
{
    //functional Api: for operational usage, e.g. reloadConfig after database updates appsettings.json
    //any data returning api is not functional Api.
    //Don't forget to add DI for IConfigurationRoot in Startup.cs
    //http://localhost:5000/api/operations/reloadconfig 
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<OperationsController> _logger;

        public OperationsController(ILogger<OperationsController> logger, IConfigurationRoot config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpOptions("reloadConfig")]
        public IActionResult ReloadConfiguration()
        {
            try
            {
                _config.Reload();

                return Ok("Configuration Reloaded");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while reloading configuration: {ex}");
            }

            return BadRequest("Could not reload configuration");
        }
    }
}