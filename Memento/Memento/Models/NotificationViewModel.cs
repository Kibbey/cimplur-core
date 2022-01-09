using Domain.Models;
using System;

namespace Memento.Web.Models
{
    public class NotificationViewModel
    {
        public NotificationsModel Notifications { get; set; }
        public String Version { get; set; }
        public bool HasPremiumPlan { get; set; }
    }
}