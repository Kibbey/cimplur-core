using System;
using System.ComponentModel.DataAnnotations;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class PromptTimeline
    {
        [Key]
        public int PromptIdTimelineId { get; set; }
        [IndexColumn("IX_PromptTimelines_PromptId_TimelineId", 0, IsUnique = true)]
        public int PromptId { get; set; }
        [IndexColumn("IX_PromptTimelines_PromptId_TimelineId", 1, IsUnique = true)]
        public int TimelineId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Prompt Prompt { get; set; }
        public virtual Timeline Timeline { get; set; }
    }
}