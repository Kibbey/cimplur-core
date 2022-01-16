using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Memento.Web.Controllers
{
    [Route("health")]
    public class HealthController : BaseApiController
    {
        public HealthController(IOptions<AppSettings> appSettings)
        {
            var settings = appSettings.Value;
            version = settings.Version;
        }

        private string version;

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(version);
        }
    }
}