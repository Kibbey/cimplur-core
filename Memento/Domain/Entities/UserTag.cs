using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class UserTag
    {
        public UserTag() {
            TagViewers = new List<TagViewer>();
            TagDrops = new List<TagDrop>();
        }
        [Key]
        public long UserTagId { get; set; }
        [IndexColumn("IX_UserId_Name", 0, IsUnique = true)]
        public int UserId { get; set; }
        public bool IsTask { get; set; }
        public bool Archived { get; set; }
        [MaxLength(100)]
        [IndexColumn("IX_UserId_Name", 1, IsUnique = true)]
        public string Name { get; set; }
        public DateTime? Created { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile Owner { get; set; }
        public virtual List<TagViewer> TagViewers { get; set; }
        public virtual List<TagDrop> TagDrops { get; set; }
    }
}
