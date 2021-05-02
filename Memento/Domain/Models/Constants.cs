using System;

namespace Domain.Models
{
    public static class Constants
    {
        public static bool InProduction = false; //Convert.ToBoolean(ConfigurationManager.AppSettings["Production"] ?? "false");
        // TODO - configuration!

        public readonly static string BaseUrl = !InProduction ? "http://localhost:62102" : "https://app.Fyli.com";
        public readonly static decimal PremiumPlanCost = 39.00m;
        public readonly static string Email = "information@fyli.com";
    }
}
