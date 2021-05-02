using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.Models
{
    public class DropModel
    {
        public DropModel() {
            UserTagDrops = new HashSet<GroupDropModel>();
            Droplets = new List<DropletModel>();
            Images = new List<int>();
            ImageLinks = new Dictionary<int, string>();
            Movies = new List<int>();
            MovieLinks = new Dictionary<int, string>();
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
        public Dictionary<int, string> ImageLinks { get; set; }
        public IEnumerable<int> Movies { get; set; }
        public Dictionary<int, string> MovieLinks { get; set; }
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
