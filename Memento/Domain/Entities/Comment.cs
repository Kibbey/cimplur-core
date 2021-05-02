using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Comment
    {
        public Comment() {
            Images = new List<ImageDrop>();
            Movies = new List<MovieDrop>();
        }

        [Key]
        public int CommentId { get; set; }
     
        public int DropId { get; set; }
        public int UserId { get; set; }
        [MaxLength(1000)]
        public string Content { get; set; }
        public KindOfComments Kind { get; set; }
        public DateTime TimeStamp { get; set; }

        public virtual Drop Drop { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile Owner { get; set; }
        public virtual List<ImageDrop> Images { get; set; }
        public virtual List<MovieDrop> Movies { get; set; }
    }
}
