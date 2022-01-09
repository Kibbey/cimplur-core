using System.Collections.Generic;

namespace Bridge
{
    public class GroupViewersModel
    {
        public GroupViewersModel() {
        }

        public int OwnerId { get; set; }
        public TagViewersModel TagViewers { get; set; }
    }
}
