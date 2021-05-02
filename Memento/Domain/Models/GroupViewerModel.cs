using System.Collections.Generic;

namespace Domain.Models
{
    public class GroupViewersModel
    {
        public GroupViewersModel() {
        }

        public int OwnerId { get; set; }
        public TagViewersModel TagViewers { get; set; }
    }
}
