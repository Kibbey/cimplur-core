using Memento.Web.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Microsoft.Extensions.Options;
using Domain.Models;
using Domain.Repository;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/notifications")]
    public class NotificationController : BaseApiController
    {
        private NotificationService notificationService;
        private UserService userService;
        public NotificationController(IOptions<AppSettings> appSettings, 
            NotificationService notificationService,
            UserService userService)
        {
            var settings = appSettings.Value;
            version = settings.Version;
            this.notificationService = notificationService;
            this.userService = userService;
        }

        private string version;

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            var notifcationModel = new NotificationViewModel();
            notifcationModel.Notifications = notificationService.Notifications(CurrentUserId);
            notifcationModel.Version = version;
            var user = await userService.GetProfile(CurrentUserId);
            notifcationModel.HasPremiumPlan = user.PremiumMember;
            return Ok(notifcationModel);
        }


        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> RemoveAll()
        {
            notificationService.RemoveAllNotifications(CurrentUserId);
            return Ok();
        }

        [HttpPut]
        [Route("{dropId}")]
        public async Task<IActionResult> Viewed(int dropId)
        {
            notificationService.ViewNotification(CurrentUserId, dropId);
            return Ok();
        }
    }
}