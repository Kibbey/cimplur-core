using System;

namespace Domain.Models
{
    public static class Constants
    {
        private static string ProdValue = Environment.GetEnvironmentVariable("PRODUCTION");
        public static bool InProduction = ProdValue == "true";

        public readonly static string HostUrl = !InProduction ? "http://localhost:8000" : "https://app.Fyli.com";
        public readonly static string BaseUrl = !InProduction ? "https://localhost:4040" : "https://app.Fyli.com";
        //public readonly static string HostUrl = !InProduction ? "http://localhost:8000" : "https://cimplur.com";
        //public readonly static string BaseUrl = !InProduction ? "https://localhost:4040" : "https://cimplur.com";
        public readonly static decimal PremiumPlanCost = 39.00m;
        public readonly static string Email = "information@fyli.com";
    }
}
