
using Memento.Web.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/contacts")]
    public class ContactController : BaseApiController
    {

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveMessage(ContactUsModel model)
        {
            if (ModelState.IsValid)
            {
                await ContactService.SendMessage(model.Email, model.Name, System.Web.HttpUtility.HtmlEncode(model.Content), CurrentUserId);
                return Ok();
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("Please include a name and valid email.");
        }


        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> SendEmail(string id)
        {
            if (id == emailMessage) {
                await ContactService.SendEmailToUsers();
            }
            return Ok();
        }

        readonly static string emailMessage = "fyli_test";
    }
}