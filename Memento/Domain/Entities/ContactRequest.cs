using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ContactRequest
    {
        [Key()]
        public int ContactRequestId { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(3500)]
        public string Content { get; set; }
        public int UserId { get; internal set; }
    }
}
