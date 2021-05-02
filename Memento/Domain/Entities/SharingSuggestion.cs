using Domain.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class SharingSuggestion
    {
        // fluent
        //[Key, Column(Order = 0)]
        public int OwnerUserId { get; set; }
        //[Key, Column(Order = 1)]
        public int SuggestedUserId { get; set; }
        public decimal Points { get; set; }
        [MaxLength(150), Column(TypeName = "varchar")]
        public string Reason { get; set; }
        public DateTime? Resolved { get; set; }
        public Resolutions Resolution { get; set; }
        public DateTime CreatedAt { get; set; }

        //[ForeignKey("OwnerUserId")]
        //[InverseProperty("SharingSuggestions")]
        public virtual UserProfile OwnerUser { get; set; }
        //[ForeignKey("SuggestedUserId")]
        //[InverseProperty("SuggestedTo")]
        public virtual UserProfile SuggestedUser { get; set; }
    }
}
