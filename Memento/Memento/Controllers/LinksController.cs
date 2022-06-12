using Domain.Models;
using Domain.Utilities;
using log4net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Domain.Repository;
using Memento.Models;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/links")]
    public class LinksController : BaseApiController
    {
        private UserService userService;
        private UserWebToken _userWebToken;
        private TokenService tokenService;
        public LinksController(UserWebToken userWebToken, UserService userService, TokenService tokenService)
        {
            _userWebToken = userWebToken;
            this.userService = userService;
            this.tokenService = tokenService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(string token)
        {
            var routeWithToken = EmailSafeLinkCreator.UnencodeLink(token);
            var loginToken = await Login(routeWithToken);
            var path = EmailSafeLinkCreator.GetPath(routeWithToken);
            if (loginToken == null) {
                return Redirect(Constants.HostUrl);
            } else {
                return Redirect(CreateRoute(path, loginToken));
            }
        }

        private string CreateRoute(string newRoute, string token) {
            var result = $"{Constants.HostUrl}/#/links?route={newRoute}&token={token}";
            return result;
        }

        private async Task<string> Login(string route) {
            if (route == null || route.Length > 300) return null;
            try {
                var linkToken = EmailSafeLinkCreator.RetrieveLink(route);
                if (!string.IsNullOrWhiteSpace(linkToken))
                {
                    var userId = await tokenService.ValidateToken(linkToken);
                    if (userId.HasValue && CurrentUserId != userId) {
                        return _userWebToken.generateJwtToken(userId.Value);
                    }
                }
            } catch (Exception e) {
                logger.Error($"Link Controller -", e);
            }
            return null;
        }

        private ILog logger = LogManager.GetLogger(nameof(LinksController));
    }
}