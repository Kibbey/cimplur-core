using System.Collections.Generic;

namespace Bridge
{
    public class ContentModel
    {
        public ContentModel(int contentId, string stuff)
        {
            this.ContentId = contentId;
            this.Stuff = stuff;
        }
        public ContentModel() { }
        public int ContentId { get; set; }
        public string Stuff { get; set; }
        public List<ContentSplitModel> SplitStuff { get; set; }
    }
}