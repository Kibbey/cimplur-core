using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Domain.Models;

namespace Memento.Libs
{
    public class AuthMiddleWare { 
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private readonly UserWebToken _userWebToken;

        public AuthMiddleWare(RequestDelegate next, IOptions<AppSettings> appSettings, UserWebToken userWebToken)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = CookieHelper.GetAuthToken(context);
                // context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            
            if (token != null)
            {
                attachUserToContext(context, token);
            }

            await _next(context);
        }

        private void attachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                var newToken = _userWebToken.generateJwtToken(userId);
                CookieHelper.SetAuthToken(newToken, context);
                // attach user to context on successful jwt validation
                context.Items["UserId"] = userId;
            }
            catch
            {
                // throw new 401
            }
        }
    }
}

