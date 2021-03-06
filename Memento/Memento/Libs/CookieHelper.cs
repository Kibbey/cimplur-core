using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
//using System.Text.Json;

namespace Memento.Libs
{
    public static class CookieHelper
    {
        private static string AUTH_COOKIE = "fyli_auth";  
        public static void SetCookie<T>(string name, T data, HttpContext httpContext)
        {
            string value = JsonConvert.SerializeObject(data);
            var options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(1);
            options.IsEssential = true;
            // TODO - fix this as a dev feature
            //options.HttpOnly = true;
            httpContext.Response.Cookies.Append(name, value, options);
        }


        public static T GetCookie<T>(string name, HttpContext httpContext) {
            var cookie = httpContext.Request.Cookies[name];
            T value = default(T);
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie)) {
                value = JsonConvert.DeserializeObject<T>(cookie);
            }
            return value;
        }

        public static void SetAuthToken(string token, HttpContext httpContext)
        {

            var options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(30);
            //options.HttpOnly = true;
            options.IsEssential = true;
            //options.SameSite = SameSiteMode.Strict;
            //options.Secure = true;
            token = "test";
            httpContext.Response.Cookies.Append(AUTH_COOKIE, token, options);
        }


        public static string GetAuthToken(HttpContext httpContext)
        {
            return httpContext.Request.Cookies[AUTH_COOKIE];
        }

        public static void LogOut(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete(AUTH_COOKIE);
        }
    }
}