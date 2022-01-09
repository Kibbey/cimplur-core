using Domain.Models;
using Memento.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/connections")]
    public class ConnectionController : BaseApiController
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Connections() 
        {
            return Ok(SharingService.GetConnections(CurrentUserId));
        }

        [HttpGet]
        [Route("requests")]
        public async Task<IActionResult> ConnectionRequests()
        {
            return Ok(SharingService.GetConnectionRequests(CurrentUserId));
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
                var result = await SharingService.RequestConnection(CurrentUserId, request);
                return Ok(new { message = result.Message, isValid = result.Success});
            }
            return Ok(new { message, isValid, key });
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            SharingService.RemoveConnection(CurrentUserId, id);
            return Ok();
        }

        [HttpGet]
        [Route("inviations")]
        public async Task<IActionResult> OutstandingInviations() {
            return Ok(await SharingService.GetExistingRequests(CurrentUserId, 0, 0));
        }

        [HttpPost]
        [Route("requests/{id}/remind")]
        public async Task<IActionResult> Remind(int id)
        {
            await SharingService.RequestReminder(CurrentUserId, id);
            return Ok();
        }


        [HttpDelete]
        [Route("requests/{id}")]
        public async Task<IActionResult> CancelOutstandingInvitation(int id) {
            await SharingService.CancelRequest(CurrentUserId, id);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/suggestions")]
        public async Task<IActionResult> AddSuggestion(int id, LongCollectionModel model)
        {
            var result = await SharingService.RequestSuggestedConnection(CurrentUserId, id, model.Ids);
            return Ok(new { message = result.Message, isValid = result.Success });
        }

        [HttpDelete]
        [Route("{id}/suggestions")]
        public async Task<IActionResult> IgnoreSuggestion(int id)
        {
            await SharingService.Ignore(CurrentUserId, id);
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
            UserService.EnableEmailNotificationsOfPosts(CurrentUserId, id);
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
            return Ok(await SharingService.ConfirmationSharingRequest(model.Token, CurrentUserId, model.Name));
        }

        [HttpDelete]
        [Route("invitations/{token}/ignore")]
        public async Task<IActionResult> IgnoreConnection(string token)
        {
            await SharingService.IgnoreRequest(token, CurrentUserId);
            return Ok();
        }

        [HttpPut]
        [Route("{id}/name")]
        public async Task<IActionResult> ChangeName(int id, NameRequest model)
        {
            string name = await SharingService.UpdateName(CurrentUserId, id, model.Name);
            return Ok(new { Name = name });
        }

        [HttpGet]
        [Route("suggestions")]
        public async Task<IActionResult> Suggestions() {
            return Ok(await SharingService.GetSuggestions(CurrentUserId));
        }

        [HttpGet]
        [Route("networks/{id}")]
        public async Task<IActionResult> Networks(int id)
        {
            var viewerTags = await GroupsService.GetViewerNetworksModels(CurrentUserId, id);
            return Ok(viewerTags);
        }
    }
}