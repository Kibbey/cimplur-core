using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        [IndexColumn("IX_Transaction_UserId")]
        public int UserId { get; set; }
        public DateTime Created { get; set; }
        public int AmountCents { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        [MaxLength(100)]
        public string ChargeId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile User { get; set; }
        [InverseProperty("Transaction")]
        public virtual List<PremiumPlan> PremiumPlans { get; set; }
    }
}
