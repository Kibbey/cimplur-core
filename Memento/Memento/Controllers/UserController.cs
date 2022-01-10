using Domain.Models;
using Domain.Emails;
using Domain.Exceptions;
using Domain.Repository;
using log4net;
using Memento.Models;
using Memento.Web.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Domain.Emails.EmailTemplates;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [Route("api/users")]
    public class UserController : BaseApiController
    {
        private UserWebToken _userWebToken;
        public UserController(UserWebToken userWebToken) {
            _userWebToken = userWebToken;
        }

        [CustomAuthorization]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await UserService.GetUser(CurrentUserId));
        }

        [CustomAuthorization]
        [HttpGet]
        [Route("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            return Ok(await UserService.GetProfile(CurrentUserId));
        }

        [CustomAuthorization]
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> ChangeName(NameRequest nameModel)
        {
            return Ok(await UserService.ChangeName(CurrentUserId, nameModel.Name));
        }

        [CustomAuthorization]
        [HttpPut]
        [Route("{privateMode}/private")]
        public async Task<IActionResult> ChangePrivateMode(bool privateMode)
        {
            return Ok(await UserService.UpdatePrivateMode(CurrentUserId, privateMode));
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> CreatePassword(EmailModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Email) && IsValidEmail(model.Email))
            {
                var token = await UserService.CreateLinkToken(model.Email);
                if (token.Success) {
                    await SendEmailService.SendAsync(model.Email, EmailTypes.Login, new { token.Token, token.Name });
                }
                return Ok(new { Message = "Please check your email for your log in link. If you do not see it check your Spam folder." });
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("Please submit a valid email.");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(Models.TokenModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Token)) {
                var userId = await UserService.ValidateToken(model.Token);
                if (userId.HasValue) {
                    try {
                        var token = _userWebToken.generateJwtToken(userId.Value);
                        CookieHelper.SetAuthToken(token, HttpContext);
                        return Ok();
                    } catch (Exception e) {
                        logger.Error($"Login Controller {userId}", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("Your link appears to be expired - please request a new one.");
        }

        [HttpPost]
        [Route("link")]
        public async Task<IActionResult> Link(Models.TokenModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Token))
            {
                var userId = await UserService.ValidateToken(model.Token);
                if (userId.HasValue && CurrentUserId != userId)
                    try
                    {
                        var token = _userWebToken.generateJwtToken(userId.Value);
                        CookieHelper.SetAuthToken(token, HttpContext);
                        return Ok();
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Login Controller {userId}", e);
                    }
                return Ok();
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("Invalid");
        }

        [HttpGet]
        [Route("reasons")]
        public async Task<IActionResult> Reasons()
        {
            var reasons = CookieHelper.GetCookie<ReasonsModel>(reasonsCookie, HttpContext);
            if (reasons == null) {
                reasons = new ReasonsModel();
                if (GetIsSelected()) {
                    reasons.Reasons.Add(new ReasonModel
                    {
                        Value = "Preserve memories",
                        Key = 1
                    });
                    reasons.Reasons.Add(new ReasonModel
                    {
                        Value = "Share family memories",
                        Key = 2
                    });
                    reasons.Reasons.Add(new ReasonModel
                    {
                        Value = "Document family history",
                        Key = 3
                    });
                    reasons.Reasons.Add(new ReasonModel
                    {
                        Value = "Other",
                        Key = 4
                    });
                }
            }
            return Ok(reasons);
        }

        private Random randomGenerator = new Random();
        private bool GetIsSelected()
        {
            int number = randomGenerator.Next(2);
            return number == 1;
        }

        [HttpPost]
        [Route("reasons")]
        public async Task<IActionResult> SaveReasons(ReasonsModel model)
        {
            CookieHelper.SetCookie(reasonsCookie, model, HttpContext);
            foreach (var item in model.Reasons) {
                if (item.Selected && CurrentUserId < 0) EventService.EmitEvent(item.Value);
            }
            return Ok(model);
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {

            if (!IsValidEmail(model.Email))
            {
                return BadRequest("Please enter a valid email.");
            }

            if (UserService.CheckEmail(model.Email))
            {
                return BadRequest("Email already exists. Please log into your existing account.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest("Name is required. It is how others on Fyli see who made a comments and find you to share memories.");
            }

            if (!model.AcceptTerms)
            {
                return BadRequest("To use Fyli you must accept the Terms of Service.");
            }

            var password = Guid.NewGuid().ToString();
            var userName = model.Email;
            var reasons = Libs.CookieHelper.GetCookie<ReasonsModel>(reasonsCookie, HttpContext);
            int userId = await UserService.AddUser(model.Email, userName, model.Token, model.AcceptTerms, model.Name, reasons?.Reasons);
            var token = _userWebToken.generateJwtToken(userId);
            CookieHelper.SetAuthToken(token, HttpContext);
            GroupsService.AddHelloWorldNetworks(userId);
            await DropsService.AddHelloWorldDrop(userId);
            if (string.IsNullOrWhiteSpace(model.ReturnUrl))
            {
                model.ReturnUrl = "/#/";
            }

            try
            {
                var Token = await UserService.CreateLinkToken(model.Email);
                Task.Run(() =>
                {
                    SendEmailService.SendAsync(Constants.Email, EmailTemplates.EmailTypes.SignUp, new { model.Name });
                    SendEmailService.SendAsync(model.Email, EmailTemplates.EmailTypes.Welcome, new { model.Name, Token.Token });
                });
            }
            catch (Exception e)
            {
                // TODO - Add back
                //logger.Error("Send Emails", e);
            }
            return Created("", userId);
        }

        [HttpPost]
        [Route("logOff")]
        public async Task<IActionResult> LogOff()
        {
            LogOffUser();
            return Ok();
        }



        [CustomAuthorization]
        [HttpGet]
        [Route("plans")]
        public async Task<IActionResult> GetAvailablePlanCount()
        {
            return Ok(new { Count = await PlanService.GetAvaliableFamilyPlanCount(CurrentUserId) });
        }

        [HttpGet]
        [Route("shareRequest/{token}")]
        public async Task<IActionResult> ShareRequest(string token)
        {
            var requestor = SharingService.GetSharingRequest(token);
            if (requestor != null)
            {
                if (!requestor.TargetUserId.HasValue || CurrentUserId != requestor.TargetUserId.Value)
                {
                    // log the off to make sure they need to log back in
                    LogOffUser();
                }
                return Ok(new { Name = requestor.RequestorName });
            }
            return BadRequest("Oops we can not find your connection request.");
        }

        [CustomAuthorization]
        [HttpPost]
        [Route("shareRequest/{token}/confirm")]
        public async Task<IActionResult> ShareRequestConfirmation(string token)
        {
            try
            {
                return Ok(await SharingService.ConfirmationSharingRequest(token, CurrentUserId, string.Empty));
            }
            catch (NotFoundException ex)
            {
                return BadRequest("Oops we can not find your connection request.");
            }
        }

        [CustomAuthorization]
        [HttpGet]
        [Route("seed")]
        public async Task<IActionResult> ShareRequestSeed()
        {
            await SharingService.ProcessSharing();
            return Ok("success");
        }

        [CustomAuthorization]
        [HttpGet]
        [Route("relationships")]
        public async Task<IActionResult> GetRelationships()
        {
            return Ok(await UserService.GetRelationships(CurrentUserId));
        }

        [CustomAuthorization]
        [HttpPost]
        [Route("relationships")]
        public async Task<IActionResult> SetRelationships(SelectedIdModel selectedModel)
        {
            return Ok(await UserService.UpdateRelationships(selectedModel.Ids, CurrentUserId));
        }


        [HttpPost]
        [Route("giveFeedback")]
        public async Task<IActionResult> GiveFeedback(Models.TokenModel token) {
            string Name = await UserService.GiveFeedbackReceived(token.Token);
            try
            {
                await SendEmailService.SendAsync(Constants.Email, EmailTemplates.EmailTypes.Feedback, new { Name });
            }
            catch (Exception e)
            {
                logger.Error("Send Emails", e);
            }
            return Ok(new { Name });
        }

        // update what groups a user can view
        [CustomAuthorization]
        [HttpPut]
        [Route("{id}/groups")]
        public async Task<IActionResult> UpdateSharedTags(int id, LongCollectionModel model)
        {
            return Ok(await GroupsService.UpdateViewerNetworks(CurrentUserId, id, model.Ids));
        }

        private void LogOffUser()
        {
            CookieHelper.LogOut(HttpContext);
        }


        private bool IsValidEmail(string email)
        {
            return Domain.Utilities.TextFormatter.IsValidEmail(email);
        }

        private string reasonsCookie = "reasons";

        private ILog logger = LogManager.GetLogger(nameof(UserController));
    }
}