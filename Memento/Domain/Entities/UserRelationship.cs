using Domain.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class UserRelationship
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public Relationships Relationship { get; set; }
        public DateTime CreatedAt { get; internal set; }

        public virtual UserProfile User { get; set; }
    }
}