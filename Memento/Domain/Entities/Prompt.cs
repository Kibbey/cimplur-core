using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Prompt
    {
        public Prompt() {
            UserPrompts = new List<UserPrompt>();
            Drops = new List<Drop>();
            PromptTimelines = new List<PromptTimeline>();
        }

        [Key]
        public int PromptId { get; set; }
        [MaxLength(1000)]
        public string Question { get; set; }
        public int Order { get; set; }
        public Relationships? Relationship { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Template { get; set; }
        public int? UserId { get; set; }

        public virtual List<Drop> Drops { get; set; }
        public virtual List<UserPrompt> UserPrompts { get; set; }
        public virtual List<PromptTimeline> PromptTimelines { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile Owner { get; set; }
    }
}
