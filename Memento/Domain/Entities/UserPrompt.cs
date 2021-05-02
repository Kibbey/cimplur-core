using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class UserPrompt
    {
        public UserPrompt(){
            Askers = new List<UserPromptAsker>();
        }

        [Key]
        public long UserPromptId { get; set; }
        [IndexColumn("IX_UserPrompt_UserId")]
        public int UserId { get; set; }
        [IndexColumn("IX_UserPrompt_PromptId")]
        public int PromptId { get; set; }
        public DateTime? Used { get; set; }
        public DateTime? Dismissed { get; set; }
        [IndexColumn]
        public DateTime? LastSeen { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile User { get; set; }
        [ForeignKey("PromptId")]
        public virtual Prompt Prompt { get; set; }
        //[InverseProperty("UserPrompt")]
        public virtual List<UserPromptAsker> Askers { get; set; }
    }
}
