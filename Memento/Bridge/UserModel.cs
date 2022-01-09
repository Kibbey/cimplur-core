
using System;
using System.Collections.Generic;

namespace Bridge
{
    public class UserModel
    {
        public UserModel() {
            Variants = new Dictionary<string, string>();
        }
        
        public string Name { get; set; }
        public bool PremiumMember { get; set; }
        public string Email { get; set; }
        public bool PrivateMode { get; set; }
        public DateTime? CanShareDate { get; set; }
        public Dictionary<string, string> Variants { get; set; }
    }
}
