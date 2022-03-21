namespace Domain.Models
{
    public class MovieModel
    {
        public MovieModel(string link, int id, string thumbLink) {
            this.Link = link;
            this.Id = id;
            this.ThumbLink = thumbLink;
        }
        public string Link { get; set; }
        public int Id { get; set; }
        public string ThumbLink { get; set; }
    }
}
