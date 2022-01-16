using System.Threading.Tasks;
using Domain.Repository;
using Memento.Libs;
using Microsoft.AspNetCore.Mvc;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/memories")]
    public class MemoryController : BaseApiController
    {
        private DropsService dropService;
        public MemoryController(DropsService dropsService) {
            this.dropService = dropsService;
        }

        [HttpGet]
        [Route("notification/{notificationId}")]
        public async Task<IActionResult> GetByNotification(int notificationId)
        {
            return Ok(dropService.GetDropFromNotification(notificationId, CurrentUserId));
        }
    }
}