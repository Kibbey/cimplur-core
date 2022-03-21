using System;
using System.Collections.Generic;


namespace Domain.Models
{
    public class DropModel
    {
        public DropModel() {
            UserTagDrops = new HashSet<GroupDropModel>();
            Droplets = new List<DropletModel>();
            Images = new List<int>();
            ImageLinks = new List<ImageModel>();
            Movies = new List<int>();
            MovieLinks = new List<string>();
            Comments = new HashSet<CommentModel>();
        }

        public int DropId { get; set; }
        public int UserId { get; set; }
        public string CreatedBy { get; set; }
        public bool Archived { get; set; }
        public int ContentId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Completed { get; set; }
        public DateTypes DateType { get; set; }
        public string CompletedBy { get; set; }
        public int? CompletedByUserId { get; set; }
        public bool IsTask { get; set; }
        public IEnumerable<int> Images { get; set; }
        public List<ImageModel> ImageLinks { get; set; }
        public IEnumerable<int> Movies { get; set; }
        public List<string> MovieLinks { get; set; }
        public IEnumerable<GroupModel> Tags { get; set; }
        public bool Editable { get; set; }
        public PromptModel Prompt { get; set; }

        public virtual ICollection<GroupDropModel> UserTagDrops { get; set; }
       
        public virtual ContentModel Content { get; set; }
        public virtual List<DropletModel> Droplets { get; set; }
        public virtual IEnumerable<CommentModel> Comments { get; set; }
        public int CreatedById { get; set; }
        public DateTime OrderBy { get; set; }
        public bool HasAlbums { get; set; }
        public TimelineModel Timeline { get; set; }
    }
}
