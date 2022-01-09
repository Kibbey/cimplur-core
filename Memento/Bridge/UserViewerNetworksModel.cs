using System.Collections.Generic;

namespace Bridge
{
    public class UserViewerNetworksModel
    {
        public UserViewerNetworksModel()
        {
            ViewerTags = new List<GroupModel>();
        }

        public int OwnerId { get; set; }
        public int ViewerId { get; set; }
        public List<GroupModel> ViewerTags { get; set; }
    }
}
