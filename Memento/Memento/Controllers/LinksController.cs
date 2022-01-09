﻿using Domain.Models;
using Domain.Utilities;
using log4net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/links")]
    public class LinksController : BaseApiController
    {
        private UserWebToken _userWebToken;
        public LinksController(UserWebToken userWebToken)
        {
            _userWebToken = userWebToken;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(string token)
        {
            var newRoute = EmailSafeLinkCreator.UnencodeLink(token);
            await Login(newRoute);
            return Redirect(Constants.BaseUrl + newRoute);
        }

        private async Task Login(string route) {
            if (route == null || route.Length > 300) return;
            try {
                var linkToken = EmailSafeLinkCreator.RetrieveLink(route);
                if (!string.IsNullOrWhiteSpace(linkToken))
                {
                    var userId = await UserService.ValidateToken(linkToken);
                    if (userId.HasValue && CurrentUserId != userId) {
                        var token = _userWebToken.generateJwtToken(userId.Value);
                        CookieHelper.SetAuthToken(token, HttpContext);
                    }
                }
            } catch (Exception e) {
                logger.Error($"Link Controller -", e);
            }
        }

        private ILog logger = LogManager.GetLogger(nameof(LinksController));
    }
}