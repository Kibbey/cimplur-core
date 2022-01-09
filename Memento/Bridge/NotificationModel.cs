using System;

namespace Bridge
{
    public class NotificationModel
    {
        public string Name { get; set; }
        public int DropId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Viewed { get; set; }
        public DateTime? ViewedAt { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
