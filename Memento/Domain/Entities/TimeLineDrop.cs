using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TimelineDrop
    {
        public TimelineDrop() { }
        // fluent
        //[Column(Order = 0), Key]
        public int TimelineId { get; set; }
        //[Column(Order = 1), Key]
        public int DropId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Timeline Timeline { get; set; }
        public virtual Drop Drop { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile UserProfile { get; set; }
    }
}

