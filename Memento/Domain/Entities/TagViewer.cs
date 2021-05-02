using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TagViewer
    {
        // fluent
        //[Column(Order = 0),Key]
        public long UserTagId { get; set; }
        //[Column(Order = 1), Key]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile Viewer { get; set; }

        [ForeignKey("UserTagId")]
        public UserTag UserTag { get; set; }
    }
}
