using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TimelineUser
    {
        [Key]
        public int TimelineUserId { get; set; }
        public int TimelineId { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        public bool Active { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile User { get; set; }
        public virtual Timeline Timeline { get; set; }
    }
}
