using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Utilities
{
    public class EmailSafeLinkCreator
    {
        public static string ConvertLink(string link) 
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(link);
            return  "/api/links?token=" + WebUtility.UrlEncode(Convert.ToBase64String(plainTextBytes));
        }

        public static string FindAndReplaceLinks(string emailBody) {
            return Regex.Replace(emailBody, pattern, m => ConvertLink(m.Groups[0].Value));
        }

        public static string UnencodeLink(string route) {
            var buffer = Convert.FromBase64String(route);
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        public static string RetrieveLink(string route) {
            var match =  Regex.Match(route, linkPattern);
            return WebUtility.UrlDecode(match.Value);
        }

        public static string GetPath(string route) {
            return WebUtility.UrlEncode(route.Replace(pathPattern, ""));
        }

        private static string pattern = @"\/#\/.+?(?=')";
        private static string linkPattern = @"(?<=link=).+?((?=&)|\z)";
        private static string pathPattern = @"#/";

    }
}
