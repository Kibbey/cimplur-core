using System;

namespace Bridge
{
    public class OutstandingConnectionRequests
    {
        public string ContactName { get; set; }
        public string Email { get; set; }
        public int RequestId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool CanSend { get; set; }
        public bool PlanInvite { get; set; }
    }
}
