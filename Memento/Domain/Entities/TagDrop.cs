using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class TagDrop
    {
        [Key]
        public int TagDropId { get; set; }
        [IndexColumn("IX_TagDrop_UserTagId_DropId", 1, IsUnique = true)]
        public int DropId { get; set; }
        [IndexColumn("IX_TagDrop_UserTagId_DropId", 0, IsUnique = true)]
        public long UserTagId { get; set; }      

        [ForeignKey("DropId")]
        public virtual Drop Drop { get; set; }

        [ForeignKey("UserTagId")]
        public virtual UserTag UserTag { get; set; }

    }
}
