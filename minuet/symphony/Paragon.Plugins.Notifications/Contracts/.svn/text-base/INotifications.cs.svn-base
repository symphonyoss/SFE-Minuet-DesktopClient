using System;

namespace Paragon.Plugins.Notifications.Contracts
{
    public interface INotifications
    {
        event EventHandler<CloseNotificationEventArgs> Closed;
        event EventHandler<ClickNotificationEventArgs> Clicked;

        void ClearAll();
        void ClearGroup(string group);
        string Create(NotificationOptions options);
        string Create(string notificationId, NotificationOptions options);
    }
}