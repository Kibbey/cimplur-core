using System.Collections.Generic;

namespace Domain.Models
{
    public class NotificationsModel
    {
        public NotificationsModel() {
            Notifications = new List<NotificationModel>();
            NotificationHash = string.Empty;
        }
        public List<NotificationModel> Notifications { get; set; }
        public string NotificationHash { get; set; }
    }
}
