using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entities
{
    public class SharedDropNotification
    {
        public SharedDropNotification() 
        {
        
        }

        public int SharedDropNotificationId {get; set;}
        public int SharerUserId { get; set; }
        public int TargetUserId { get; set; }
        public int? DropId { get; set; }
        public DateTime TimeShared { get; set; }

        [ForeignKey("SharerUserId")]
        //[InverseProperty("TargetNotifications")]
        public virtual UserProfile Sharer { get; set; }
        [ForeignKey("TargetUserId")]
        //[InverseProperty("MyNotifications")]
        public virtual UserProfile Target { get; set; }
        [ForeignKey("DropId")]
        [InverseProperty("Notifications")]
        public virtual Drop Drop { get; set; }
    }
}
