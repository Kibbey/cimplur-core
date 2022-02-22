using Domain.Models;
using Memento.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Domain.Repository;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/connections")]
    public class ConnectionController : BaseApiController
    {
        private SharingService sharingService;
        private UserService userService;
        private GroupService groupService;
        public ConnectionController(
            SharingService sharingService,
            UserService userService,
            GroupService groupService) {
            this.sharingService = sharingService;
            this.userService = userService;
            this.groupService = groupService;
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Connections() 
        {
            return Ok(userService.GetConnections(CurrentUserId));
        }

        [HttpGet]
        [Route("requests")]
        public async Task<IActionResult> ConnectionRequests()
        {
            return Ok(sharingService.GetConnectionRequests(CurrentUserId));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Add(AddConnectionModel model) 
        {
            bool isValid = false;
            string message = "";
            string key = "";
            if (Domain.Utilities.TextFormatter.IsValidEmail(model.Email)) {
                isValid = true;
            } else {
                message = "Please enter a valid email.";
                key = nameof(model.Email);
            }
            if (string.IsNullOrWhiteSpace(model.RequestorName))
            {
                message = "Please enter tell the person you are sharing with who you are.";
                isValid = false;
                key = nameof(model.RequestorName);
            }

            if (isValid) {
                var request = new ConnectionRequestModel
                {
                    ContactName = model.ContactName,
                    Email = model.Email,
                    Tags = model.GroupIds ?? new List<long>(),
                    RequestorName = model.RequestorName,
                    PromptId = model.PromptId,
                    TimelineId = model.TimelineId
                };
                var result = await sharingService.RequestConnection(CurrentUserId, request);
                return Ok(new { message = result.Message, isValid = result.Success});
            }
            return Ok(new { message, isValid, key });
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            sharingService.RemoveConnection(CurrentUserId, id);
            return Ok();
        }

        [HttpGet]
        [Route("inviations")]
        public async Task<IActionResult> OutstandingInviations() {
            return Ok(await sharingService.GetExistingRequests(CurrentUserId, 0, 0));
        }

        [HttpPost]
        [Route("requests/{id}/remind")]
        public async Task<IActionResult> Remind(int id)
        {
            await sharingService.RequestReminder(CurrentUserId, id);
            return Ok();
        }


        [HttpDelete]
        [Route("requests/{id}")]
        public async Task<IActionResult> CancelOutstandingInvitation(int id) {
            await sharingService.CancelRequest(CurrentUserId, id);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/suggestions")]
        public async Task<IActionResult> AddSuggestion(int id, LongCollectionModel model)
        {
            var result = await sharingService.RequestSuggestedConnection(CurrentUserId, id, model.Ids);
            return Ok(new { message = result.Message, isValid = result.Success });
        }

        [HttpDelete]
        [Route("{id}/suggestions")]
        public async Task<IActionResult> IgnoreSuggestion(int id)
        {
            await sharingService.Ignore(CurrentUserId, id);
            return Ok();
        }

        /*
        [HttpPost]
        [Route("requests")]
        public async Task<IActionResult> AddUserEmail(EmailModel emailModel)
        {
            bool isValid = false;
            try
            {
                MailAddress m = new MailAddress(emailModel.Email);
                isValid = true;
            }
            catch { }
            if (isValid)
            {
                return Ok(new { message = UserService.SendClaimEmail(CurrentUserId, emailModel.Email), isValid = true });
            }
            // this is wrong, but big enough refactor on going here
            return Ok(new { message = "Please enter a valid email.", isValid = false });
        }
        */

        [HttpPost]
        [Route("{id}/notifications")]
        public async Task<IActionResult> UpdateEmailNotification(int id)
        {
            userService.EnableEmailNotificationsOfPosts(CurrentUserId, id);
            return Ok();
        }

        /*
        [AllowAnonymous]
        public ActionResult ClaimEmail(string token)
        {
            ViewBag.Message = UserService.ClaimEmail(token);
            return View("ClaimEmail");
        }
        */

        [HttpPost]
        [Route("confirm")]
        public async Task<IActionResult> ConfirmConnection(NameRequest model) 
        {
            return Ok(await sharingService.ConfirmationSharingRequest(model.Token, CurrentUserId, model.Name));
        }

        [HttpDelete]
        [Route("invitations/{token}/ignore")]
        public async Task<IActionResult> IgnoreConnection(string token)
        {
            await sharingService.IgnoreRequest(token, CurrentUserId);
            return Ok();
        }

        [HttpPut]
        [Route("{id}/name")]
        public async Task<IActionResult> ChangeName(int id, NameRequest model)
        {
            string name = await sharingService.UpdateName(CurrentUserId, id, model.Name);
            return Ok(new { Name = name });
        }

        [HttpGet]
        [Route("suggestions")]
        public async Task<IActionResult> Suggestions() {
            return Ok(await sharingService.GetSuggestions(CurrentUserId));
        }

        [HttpGet]
        [Route("networks/{id}")]
        public async Task<IActionResult> Networks(int id)
        {
            var viewerTags = await groupService.GetViewerNetworksModels(CurrentUserId, id);
            return Ok(viewerTags);
        }
    }
}