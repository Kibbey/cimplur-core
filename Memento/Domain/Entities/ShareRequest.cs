using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ShareRequest
    {
        public int ShareRequestId { get; set; }
        public int RequesterUserId { get; set; }
        //requester gives us this
        public string RequestorName { get; set; }
        public string TargetsEmail { get; set; }
        // future contact name for user
        public string TargetAlias { get; set; }
        //this is matched if our email matches current user or after they signup
        public int? TargetsUserId { get; set; }
        //use this for the email link
        public Guid RequestKey { get; set; }
        public bool Ignored { get; set; }
        [MaxLength(8000), Column(TypeName = "varchar")]
        public string TagsToShare { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? PromptId { get; set; }
        public int? TimelineId { get; set; }
        public int? PremiumPlanId { get; set; }

        public DateTime UpdatedAt { get; set; }

        public PremiumPlan PremiumPlan { get; set; }
        [ForeignKey("RequesterUserId")]
        //[InverseProperty("MyRequests")]
        public virtual UserProfile RequestingUser { get; set; }

        [ForeignKey("TargetsUserId")]
        //[InverseProperty("OthersRequestingMe")]
        public virtual UserProfile TargetUser { get; set; }
    }
}
