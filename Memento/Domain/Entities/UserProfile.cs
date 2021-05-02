using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class UserProfile
    {
        public UserProfile() 
        {
            Drops = new List<Drop>();
            UserTags = new List<UserTag>();
            SharedWithUser = new List<UserUser>();
            ShareWithUser = new List<UserUser>();
            OtherPeoplesDrops = new List<UserDrop>();
            Comments = new List<Comment>();
            Albums = new List<Album>();
            UserPrompts = new List<UserPrompt>();
            UserRelationships = new List<UserRelationship>();
            CreatedTimeLines = new List<Timeline>();
            TimeLines = new List<TimelineUser>();
            Skip = 0;
            Created = DateTime.UtcNow;
        }

        [Key()]
        [IndexColumn("IX_UserId_PremiumExpiration", 1)]
        public int UserId { get; set; }
        public string UserName { get; set; }
        [MaxLength(100), Column(TypeName = "varchar")]
        public string Email { get; set; }
        [MaxLength(100), Column(TypeName = "varchar")]
        public string Name { get; set; }
        [MaxLength(4000), Column(TypeName = "varchar")]
        public string CurrentTagIds { get; set; }
        [MaxLength(8000), Column(TypeName = "varchar")]
        public string CurrentNotifications { get; set; }
        [MaxLength(8000), Column(TypeName = "varchar")]
        public string CurrentPeople { get; set; }
        [MaxLength(8000), Column(TypeName = "varchar")]
        public string CurrentSuggestedPeople { get; set; }
        public int? Skip { get; set; }
        public bool? Me { get; set; }
        public bool NotififySuggestions { get; set; }
        public bool PrivateMode { get; set; }
        [IndexColumn("IX_UserId_PremiumExpiration", 2)]
        public DateTime? PremiumExpiration { get; set; }
        public DateTime? AcceptedTerms { get; set; }
        public DateTime? SuggestionUpdated { get; set; }
        [MaxLength(4000), Column(TypeName = "varchar")]
        public string Reasons { get; set; }
        [MaxLength(128)]
        public string Token { get; set; }
        public DateTime? TokenCreation { get; set; }
        public int TokenAttempts { get; set; }
        public DateTime? GiveFeedback { get; internal set; }
        public DateTime Created { get; set; }
        public DateTime SuggestionReminderSent { get; set; }
        public DateTime QuestionRemindersSent { get; set; }

        //created
        public virtual List<Drop> Drops { get; set; }
        public virtual List<UserTag> UserTags { get; set; }
        // The users I share with
        public virtual List<UserUser> ShareWithUser { get; set; }
        // The users that share with you
        public virtual List<UserUser> SharedWithUser { get; set; }
        public virtual List<UserDrop> OtherPeoplesDrops { get; set; }
        public virtual List<UserEmail> AlternateEmails { get; set; }
        public virtual List<ShareRequest> MyRequests { get; set; }
        public virtual List<ShareRequest> OthersRequestingMe { get; set; }
        public virtual List<SharingSuggestion> SharingSuggestions { get; set; }
        public virtual List<SharingSuggestion> SuggestedTo { get; set; }
        public virtual List<SharedPlan> SharedPlans { get; set; }
        public virtual List<Album> Albums { get; set; }
        //completed (even if they aren't yours
        public virtual ICollection<Drop> Completed { get; set; }

        //[InverseProperty("Owner")]
        public virtual List<Timeline> CreatedTimeLines { get; set; }
        [InverseProperty("User")]
        public virtual List<TimelineUser> TimeLines { get; set; }
        //[InverseProperty("UserProfile")]
        public virtual List<TimelineDrop> TimelineDrops { get; set; }
        public virtual List<SharedDropNotification> MyNotifications { get; set; }
        public virtual List<SharedDropNotification> TargetNotifications { get; set; }
        public virtual List<Comment> Comments { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
        public virtual List<PremiumPlan> PremiumPlans { get; set; }
        public virtual List<UserPrompt> UserPrompts { get; set; }
        public virtual List<UserRelationship> UserRelationships { get; set; }
    }
}
