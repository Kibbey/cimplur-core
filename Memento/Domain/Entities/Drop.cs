using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class Drop
    {
        public Drop() {
            TagDrops = new HashSet<TagDrop>();
            OtherUsersDrops = new List<UserDrop>();
            Images = new List<ImageDrop>();
            Movies = new List<MovieDrop>();
            Comments = new List<Comment>();
            Notifications = new List<SharedDropNotification>();
            AlbumDrops = new List<AlbumDrop>();
            ChildrenDrops = new List<Drop>();
        }


        public int DropId { get; set; }
        [IndexColumn("IX_Drop_ParentDropId")]
        public int? ParentDropId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public DateTypes DateType { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Completed { get; set; }
        public int? PromptId { get; set; }
        public int? TimelineId { get; set; }
        //Need to do attribute because breaks convention?
        public int? CompletedByUserId { get; set; }
        public int DayOfYear { get; set; }

        public virtual ICollection<TagDrop> TagDrops { get; set; }
        public virtual ICollection<UserDrop> OtherUsersDrops { get; set; }
        [InverseProperty("Drops")]
        [ForeignKey("UserId")]
        public virtual UserProfile CreatedBy { get; set; }
        public virtual ContentDrop ContentDrop { get; set; }
        [ForeignKey("CompletedByUserId")]
        [InverseProperty("Completed")]
        public virtual UserProfile CompletedBy { get; set; }
        
        public virtual Prompt Prompt { get; set; }
        public virtual Timeline Timeline { get; set; }
        public virtual IList<Comment> Comments { get; set; }
        public virtual IList<ImageDrop> Images { get; set; }
        public virtual IList<MovieDrop> Movies { get; set; }
        public virtual IList<SharedDropNotification> Notifications { get; set; }
        public virtual IList<AlbumDrop> AlbumDrops { get; set; }
        public virtual IList<TimelineDrop> TimelineDrops { get; set; }
        [ForeignKey("ParentDropId")]
        public virtual Drop ParentDrop { get; set; }
        [InverseProperty("ParentDrop")]
        public virtual IList<Drop> ChildrenDrops { get; set; }
        public bool Archived { get; set; }
    }
}
