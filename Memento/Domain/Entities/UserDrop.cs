using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class UserDrop
    {
        public int UserDropId { get; set; }
        public int UserId { get; set; }
        public int DropId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile User { get; set; }
        public virtual Drop Drop { get; set; }
    }
}
