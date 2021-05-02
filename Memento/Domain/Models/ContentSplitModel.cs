namespace Domain.Models
{
    public class ContentSplitModel
    {
        public string Content { get; set; }
        public ContentType ContentType { get; set; }
    }

    public enum ContentType 
    { 
        Normal = 0, 
        Link = 1
    }

}
