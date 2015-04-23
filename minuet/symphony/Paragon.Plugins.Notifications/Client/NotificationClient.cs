using System;
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications.Client
{
    public class NotificationClient : INotifications
    {
        private readonly INotifications notifications;
        private readonly INotificationWindowManager windowManager;

        public NotificationClient(LocalServiceFactory serviceFactory)
        {
            notifications = new Notifications(
                serviceFactory.EventAggregator,
                serviceFactory.NotificationFactory);

            windowManager = serviceFactory.NotificationWindowManager;
        }

        public event EventHandler<CloseNotificationEventArgs> Closed
        {
            add { notifications.Closed += value; }
            remove { notifications.Closed -= value; }
        }

        public event EventHandler<ClickNotificationEventArgs> Clicked
        {
            add { notifications.Clicked += value; }
            remove { notifications.Clicked -= value; }
        }

        public void ClearAll()
        {
            notifications.ClearAll();
        }

        public void ClearGroup(string group)
        {
            notifications.ClearGroup(group);
        }

        public string Create(NotificationOptions options)
        {
            return notifications.Create(options);
        }

        public string Create(string notificationId, NotificationOptions options)
        {
            return notifications.Create(notificationId, options);
        }

        public void ShowSettings(IApplicationWindow owner)
        {
            windowManager.ShowSettings(owner);
        }
    }
}