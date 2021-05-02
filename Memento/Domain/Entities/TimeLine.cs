using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Timeline
    {
        public Timeline() {
            TimelineUsers = new List<TimelineUser>();
            TimelineDrops = new List<TimelineDrop>();
            TimelinePrompts = new List<PromptTimeline>();
        }
        [Key]
        public int TimelineId { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile Owner { get; set; }
        public virtual List<TimelineUser> TimelineUsers { get; set; }
        public virtual List<TimelineDrop> TimelineDrops { get; set; }
        public virtual List<PromptTimeline> TimelinePrompts { get; set; }
    }
}
