using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class OneTimePasswordModel
    {
        public string Token { get; set; }
        public string Name { get; set; }
        public bool Success { get; set; }
        public string Reason { get; set; }
    }
}
