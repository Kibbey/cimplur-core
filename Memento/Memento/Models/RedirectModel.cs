namespace Memento.Models
{
    public class RedirectModel
    {
        public RedirectModel(string location, string token) {
            this.Location = location;
            this.Token = token;
        }
        public string Location { get; set; }
        public string Token { get; set; }
    }
}
