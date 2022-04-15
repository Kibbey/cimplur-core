namespace Domain.Models
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string Version { get; set; }
        public string DatabaseConnection { get; set; }
        public bool Production { get; set; }
        public string Owner { get; set; }
        public string EmailCode { get; set; }
        public string EmailToken { get; set; }
        public string Link { get; set; }
    }
}
