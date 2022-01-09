using System.Collections.Generic;

namespace Bridge
{
    public class ConnectionRequestModel
    {
        public ConnectionRequestModel() {
            Tags = new List<long>();
        }
        public string Email { get; set; }
        public string ContactName { get; set; }
        public string RequestorName { get; set; }
        public int? TargetUserId { get; set; }
        // used to be tag Id, now it is UserTagId
        public List<long> Tags { get; set; }
        public int? PromptId { get; set; }
        public int? TimelineId { get; set; }
    }
}
