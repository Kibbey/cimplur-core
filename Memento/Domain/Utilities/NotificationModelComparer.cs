using Domain.Models;
using System.Collections.Generic;

namespace Domain.Utilities
{
    public sealed class NotificationModelComparer : IEqualityComparer<NotificationModel>
    {
        public bool Equals(NotificationModel x, NotificationModel y)
        {
            return x.DropId == y.DropId && x.NotificationType == y.NotificationType;
        }

        public int GetHashCode(NotificationModel obj)
        {
            return obj.NotificationType == NotificationType.Comment ? -1 *  obj.DropId : obj.DropId;
        }
    }
}
