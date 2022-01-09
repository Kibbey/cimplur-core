using System;

namespace Bridge
{
    public class SharedPlanModel
    {
        public string SharedWith { get; set; }
        public DateTime Modified { get; set; }
        public bool Pending { get; set; }
        public bool Revoked { get; set; }
    }
}
