using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace Domain.Entities
{
    public class UserUser
    {
        public int UserUserId { get; set; }
        [IndexColumn]
        [IndexColumn("IX_UserUser_ReaderUserId_OwnerUserId", 1, IsUnique = true)]
        public int OwnerUserId { get; set; }
        [IndexColumn]
        [IndexColumn("IX_UserUser_ReaderUserId_OwnerUserId", 0, IsUnique = true)]
        public int ReaderUserId { get; set; }
        [MaxLength(100), Column(TypeName = "varchar")]
        public string ReaderName { get; set; }
        public bool Archive { get; set; }
        public bool SendNotificationEmail { get; set; }
        public bool CanReShare { get; set; }

        //[ForeignKey("OwnerUserId")]
        //[InverseProperty("ShareWithUser")]
        public virtual UserProfile OwnerUser { get; set; }
        //[ForeignKey("ReaderUserId")]
        //[InverseProperty("SharedWithUser")]
        public virtual UserProfile ReaderUser { get; set; }
    }
}
