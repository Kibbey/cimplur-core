namespace Domain.Models
{
    public class ImageModel
    {
        public ImageModel(string link, int id) {
            this.Link = link;
            this.Id = id;
        }
        public string Link { get; set; }
        public int Id { get; set; }
    }
}
