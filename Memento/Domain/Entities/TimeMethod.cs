using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class TimeMethod
    {
        [Key]
        public int Id { get; set; }
        public int Time { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public DateTime TimeStamp { get; set; }
        
    }
}
