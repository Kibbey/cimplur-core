using System.Collections.Generic;

namespace Domain.Models
{
    public class GroupModel
    {
        public GroupModel() {

        }

        public long TagId {get;set;}
        public bool IsTask { get; set; }
        public bool Archived { get; set; }
        public bool Selected { get; set; }
        public bool Foreign { get; set; }
        public bool CanNotEdit { get; set; }
        public string Name { get; set; }

    }

    public class UserTagComparer : IEqualityComparer<GroupModel>
    {
        public bool Equals(GroupModel x, GroupModel y)
        {
            if (x.TagId == y.TagId)
            {
                return true;
            }
            else { return false; }
        }
        public int GetHashCode(GroupModel model)
        {
            return (int)model.TagId;
        }

    }
}
