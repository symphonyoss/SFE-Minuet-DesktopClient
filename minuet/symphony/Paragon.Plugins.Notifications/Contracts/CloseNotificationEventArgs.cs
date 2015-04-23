using System;

namespace Paragon.Plugins.Notifications.Contracts
{
    public class CloseNotificationEventArgs : EventArgs
    {
        public CloseNotificationEventArgs(
            string notificationId,
            bool byUser)
        {
            NotificationId = notificationId;
            ByUser = byUser;
        }

        public string NotificationId { get; internal set; }
        public bool ByUser { get; internal set; }
    }
}