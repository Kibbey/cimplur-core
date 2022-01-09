

using System.ComponentModel.DataAnnotations;

namespace Memento.Web.Models
{
    public class ContactUsModel
    {
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [MaxLength(3500)]
        public string Content { get; set; }
    }
}