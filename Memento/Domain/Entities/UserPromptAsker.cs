using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entities
{
    public class UserPromptAsker
    {
        [Key]
        public long UserPromptAskerId { get; set; }
        public long UserPromptId { get; set; }
        public int AskerId { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("AskerId")]
        public virtual UserProfile Asker { get; set; }
        [ForeignKey("UserPromptId")]
        public virtual UserPrompt UserPrompt { get; set; }
    }
}
