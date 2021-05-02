using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ContentDrop
    {
        public int ContentDropId { get; set; }
        public string Stuff { get; set; }

        [Required]
        [ForeignKey("ContentDropId")]
        public virtual Drop Drop { get; set; }
    }
}
