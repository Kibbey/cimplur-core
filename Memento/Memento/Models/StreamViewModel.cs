using Domain.Models;
using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class StreamViewModel
    {
        public StreamViewModel() {
            Drops = new List<DropModel>();
            Tags = new List<GroupModel>();
            SelectedTags = new List<int>();
        }

        public List<DropModel> Drops { get; set; }
        public List<GroupModel> Tags { get; set; }
        public List<int> SelectedTags { get; set; }
    }
}