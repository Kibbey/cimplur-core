using System.Collections.Generic;

namespace Domain.Models
{
    public class TagViewersModel
    {
        public TagViewersModel() {
            Viewers = new List<GroupMember>();
        }
        public GroupModel Tag { get; set; }
        public IEnumerable<GroupMember> Viewers { get; set; }
    }
}
