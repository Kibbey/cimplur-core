using System;

namespace Bridge
{
    public class PlanModel
    {
        public DateTime ExpirationDate { get; set; }
        public int UserId { get; set; }
        public PlanTypes PlanType { get; set; }
        public int PlanId { get; set; }
    }
}
