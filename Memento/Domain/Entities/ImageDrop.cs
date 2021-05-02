namespace Domain.Entities
{
    public class ImageDrop
    {
        public ImageDrop()
        {

        }

        public int ImageDropId { get; set; }
        public int DropId { get; set; }
        public int? CommentId { get; internal set; }

        public virtual Drop Drop { get; set; }
        public virtual Comment Comment {get; set; }
    }
}
