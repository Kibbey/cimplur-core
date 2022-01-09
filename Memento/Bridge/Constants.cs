using System;
using System.Configuration;

namespace Bridge
{
    public static class Constants
    {
        public static bool InProduction = Convert.ToBoolean(ConfigurationManager.AppSettings["Production"] ?? "false");

        public readonly static string BaseUrl = !InProduction ? "http://localhost:62102" : "https://app.Fyli.com";
        public readonly static decimal PremiumPlanCost = 39.00m;
        public readonly static string Email = "information@fyli.com";
    }
}
