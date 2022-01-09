
using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class AddConnectionModel
    {
        public AddConnectionModel() {
            GroupIds = new List<long>();
        }
        public string Email { get; set; }
        public string ContactName { get; set; }
        public string RequestorName { get; set; }
        public List<long> GroupIds { get; set; }
        public int? PromptId { get; set; }
        public int? TimelineId { get; set; }
    }
}