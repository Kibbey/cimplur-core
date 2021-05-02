using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Usage
    { 
        [Key()]
        public long UsageId { get; set; }
        public int UserId { get; set; }
        [MaxLength(100), Column(TypeName = "varchar")]
        public string EventName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
