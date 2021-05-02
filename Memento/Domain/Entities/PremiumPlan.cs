using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class PremiumPlan
    {
        [Key]
        public int PremiumPlanId { get; set; }
        [IndexColumn("IX_PremiumPlan_UserId")]
        public int UserId { get; set; }
        public int? TransactionId { get; set; }
        public DateTime Created { get; set; }
        public int PlanLengthDays { get; set; }
        public PlanTypes PlanType { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int? ExtendedPlanId { get; set; }
        public int FamilyPlanCount{ get; set; }
        public int? ParentPremiumPlanId { get; internal set; }

        [ForeignKey("UserId")]
        [InverseProperty("PremiumPlans")]
        public virtual UserProfile UserProfile { get; set; }

        //[ForeignKey("TransactionId")]
        //[InverseProperty("PremiumPlans")]
        public virtual Transaction Transaction { get; set; }
        // If this was created by or extended by a SharedPlan
        //[InverseProperty("SharedPremiumPlan")]
        public virtual List<SharedPlan> PlansSharedFrom { get; set; }
        //[InverseProperty("PremiumPlan")]
        public virtual List<ShareRequest> ShareRequests { get; set; }
        [ForeignKey("ParentPremiumPlanId")]
        public virtual PremiumPlan ParentPremiumPlan { get; set; }
        //[InverseProperty("ParentPremiumPlan")]
        public virtual List<PremiumPlan> ChildrenPremiumPlans { get; set; }
    }
}
