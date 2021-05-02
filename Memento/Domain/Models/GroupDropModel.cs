

namespace Domain.Models
{
    public class GroupDropModel
    {
        public int UserTagDropId { get; set; }
        public int DropId { get; set; }
        public int UserTagId { get; set; }
        public int OwnerUserId { get; set; }
        public bool Archived { get; set; }
      
        public virtual GroupModel UserTag { get; set; }
        public virtual DropModel Drop { get; set; }
    }
}
