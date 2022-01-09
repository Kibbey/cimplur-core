using System.Threading.Tasks;
using Memento.Libs;
using Microsoft.AspNetCore.Mvc;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/memories")]
    public class MemoryController : BaseApiController
    {

        [HttpGet]
        [Route("notification/{notificationId}")]
        public async Task<IActionResult> GetByNotification(int notificationId)
        {
            return Ok(DropsService.GetDropFromNotification(notificationId, CurrentUserId));
        }
    }
}