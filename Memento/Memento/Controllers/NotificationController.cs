using Memento.Web.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Microsoft.Extensions.Options;
using Domain.Models;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/notifications")]
    public class NotificationController : BaseApiController
    {
        public NotificationController(IOptions<AppSettings> appSettings)
        {
            var settings = appSettings.Value;
            version = settings.Version;
        }

        private string version;

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            var notifcationModel = new NotificationViewModel();
            notifcationModel.Notifications = NotificationService.Notifications(CurrentUserId);
            notifcationModel.Version = version;
            var user = await UserService.GetProfile(CurrentUserId);
            notifcationModel.HasPremiumPlan = user.PremiumMember;
            return Ok(notifcationModel);
        }


        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> RemoveAll()
        {
            NotificationService.RemoveAllNotifications(CurrentUserId);
            return Ok();
        }

        [HttpPut]
        [Route("{dropId}")]
        public async Task<IActionResult> Viewed(int dropId)
        {
            NotificationService.ViewNotification(CurrentUserId, dropId);
            return Ok();
        }
    }
}