using System.Collections.Generic;

namespace Domain.Models
{
    public class ViewerNetworksModel
    {
        public ViewerNetworksModel()
        {
            Tags = new List<GroupModel>();
        }
        public List<GroupModel> Tags { get; set; }
        public PersonModelV2 Viewer { get; set; }
    }
}
