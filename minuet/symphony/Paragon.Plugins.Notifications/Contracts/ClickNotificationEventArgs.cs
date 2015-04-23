using System;

namespace Paragon.Plugins.Notifications.Contracts
{
    public class ClickNotificationEventArgs : EventArgs
    {
        public ClickNotificationEventArgs(
            string notificationId)
        {
            NotificationId = notificationId;
        }

        public string NotificationId { get; private set; }
    }
}