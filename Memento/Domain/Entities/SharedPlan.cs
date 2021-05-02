using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class SharedPlan
    {
        public SharedPlan() {
            Modified = DateTime.UtcNow;
        }
        [Key]
        public int SharedPlanId { get; set; }
        [IndexColumn("IX_SharedPlan_UserId")]
        public int UserId { get; set; }
        [IndexColumn("IX_SharedPlan_PremiumPlan")]
        // Plan created from this Shared Plan
        public int? SharedPremiumPlanId { get; set; }
        public string EmailSentTo { get; set; }
        // Who a request was sent to if not a connection yet
        public string Name { get; set; }
        public int? TransactionId { get; set; }
        // If a shared plan is never used, it can be revoked and re-used
        public bool Revoked { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        [ForeignKey("SharedPremiumPlanId")]
        public virtual PremiumPlan SharedPremiumPlan { get; set; }

        [ForeignKey("UserId")]
        // [InverseProperty("SharedPlans")]
        public virtual UserProfile UserProfile { get; set; }


    }
}
