using Domain.Models;
using Domain.Emails;
using Domain.Exceptions;
using Domain.Repository;
using Memento.Models;
using Memento.Web.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Domain.Emails.EmailTemplates;
using Memento.Libs;
using Microsoft.Extensions.Logging;

namespace Memento.Web.Controllers
{
    [Route("api/users")]
    public class UserController : BaseApiController
    {
        private UserWebToken userWebToken;
        private SendEmailService sendEmailService;
        private UserService userService;
        private GroupService groupService;
        private DropsService dropService;
        private SharingService sharingService;
        private TokenService tokenService;
        public UserController(UserWebToken userWebToken, 
            SendEmailService sendEmailService,
            UserService userService,
            GroupService groupService,
            DropsService dropsService,
            SharingService sharingService,
            TokenService tokenService, ILogger<UserController> logger) {
            this.userWebToken = userWebToken;
            this.sendEmailService = sendEmailService;
            this.userService = userService;
            this.groupService = groupService;
            this.dropService = dropsService;
            this.sharingService = sharingService;
            this.tokenService = tokenService;
            this.logger = logger;
        }

        [CustomAuthorization]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await userService.GetUser(CurrentUserId));
        }

        [CustomAuthorization]
        [HttpGet]
        [Route("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            return Ok(await userService.GetProfile(CurrentUserId));
        }

        [CustomAuthorization]
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> ChangeName(NameRequest nameModel)
        {
            return Ok(await userService.ChangeName(CurrentUserId, nameModel.Name));
        }

        [CustomAuthorization]
        [HttpPut]
        [Route("{privateMode}/private")]
        public async Task<IActionResult> ChangePrivateMode(bool privateMode)
        {
            return Ok(await userService.UpdatePrivateMode(CurrentUserId, privateMode));
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> CreatePassword(EmailModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Email) && IsValidEmail(model.Email))
            {
                var token = await tokenService.CreateLinkToken(model.Email);
                if (token.Success) {
                    await sendEmailService.SendAsync(model.Email, EmailTypes.Login, new { token.Token, token.Name });
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
                var userId = await tokenService.ValidateToken(model.Token);
                if (userId.HasValue) {
                    try {
                        var token = userWebToken.generateJwtToken(userId.Value);
                        return Ok(token);
                    } catch (Exception e) {
                        logger.LogError($"Login Controller {userId}", e);
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
                var userId = await tokenService.ValidateToken(model.Token);
                if (userId.HasValue && CurrentUserId != userId)
                    try
                    {
                        var token = userWebToken.generateJwtToken(userId.Value);
                        // CookieHelper.SetAuthToken(token, HttpContext);
                        return Ok(token);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Login Controller {userId}", e);
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

            if (userService.CheckEmail(model.Email))
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
            int userId = await userService.AddUser(model.Email, userName, model.Token, model.AcceptTerms, model.Name, reasons?.Reasons);
            var token = userWebToken.generateJwtToken(userId);
            groupService.AddHelloWorldNetworks(userId);
            await dropService.AddHelloWorldDrop(userId);
            if (string.IsNullOrWhiteSpace(model.ReturnUrl))
            {
                model.ReturnUrl = "/#/";
            }

            try
            {
                var Token = await tokenService.CreateLinkToken(model.Email);
                Task.Run(() =>
                {
                    sendEmailService.SendAsync(Constants.Email, EmailTemplates.EmailTypes.SignUp, new { model.Name });
                    sendEmailService.SendAsync(model.Email, EmailTemplates.EmailTypes.Welcome, new { model.Name, Token.Token });
                });
            }
            catch (Exception e)
            {
                // TODO - Add back
                //logger.Error("Send Emails", e);
            }
            return Created("", token);
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
            return Ok(new { Count = 0
                //await planService.GetAvaliableFamilyPlanCount(CurrentUserId) 
            });
        }

        [HttpGet]
        [Route("shareRequest/{token}")]
        public async Task<IActionResult> ShareRequest(string token)
        {
            var requestor = sharingService.GetSharingRequest(token);
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
                return Ok(await sharingService.ConfirmationSharingRequest(token, CurrentUserId, string.Empty));
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
            await sharingService.ProcessSharing();
            return Ok("success");
        }

        [CustomAuthorization]
        [HttpGet]
        [Route("relationships")]
        public async Task<IActionResult> GetRelationships()
        {
            return Ok(await userService.GetRelationships(CurrentUserId));
        }

        [CustomAuthorization]
        [HttpPost]
        [Route("relationships")]
        public async Task<IActionResult> SetRelationships(SelectedIdModel selectedModel)
        {
            return Ok(await userService.UpdateRelationships(selectedModel.Ids, CurrentUserId));
        }


        [HttpPost]
        [Route("giveFeedback")]
        public async Task<IActionResult> GiveFeedback(Models.TokenModel token) {
            string Name = await userService.GiveFeedbackReceived(token.Token);
            try
            {
                await sendEmailService.SendAsync(Constants.Email, EmailTemplates.EmailTypes.Feedback, new { Name });
            }
            catch (Exception e)
            {
                logger.LogError("Send Emails", e);
            }
            return Ok(new { Name });
        }

        // update what groups a user can view
        [CustomAuthorization]
        [HttpPut]
        [Route("{id}/groups")]
        public async Task<IActionResult> UpdateSharedTags(int id, LongCollectionModel model)
        {
            return Ok(await groupService.UpdateViewerNetworks(CurrentUserId, id, model.Ids));
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

        private ILogger logger;
    }
}