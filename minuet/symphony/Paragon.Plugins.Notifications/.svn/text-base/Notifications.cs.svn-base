using System;
using Paragon.Plugins.Notifications.Contracts;
using Paragon.Plugins.Notifications.Models;
using Paragon.Plugins.Notifications.Mvvm;
using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications
{
    public class Notifications : INotifications
    {
        private readonly IEventAggregator eventAggregator;
        private readonly INotificationFactory notificationFactory;

        public Notifications(
            IEventAggregator eventAggregator,
            INotificationFactory notificationFactory)
        {
            this.eventAggregator = eventAggregator;
            this.notificationFactory = notificationFactory;

            this.eventAggregator
                .GetEvent<NotificationEvents.CloseNotification>()
                .Subscribe(OnClosed);

            this.eventAggregator
                .GetEvent<NotificationEvents.ClickNotification>()
                .Subscribe(OnClicked);
        }

        public event EventHandler<CloseNotificationEventArgs> Closed;
        public event EventHandler<ClickNotificationEventArgs> Clicked;

        public void ClearAll()
        {
            eventAggregator
                .GetEvent<NotificationEvents.ClearAll>()
                .Publish(EmptyArgs.Empty);
        }

        public void ClearGroup(string group)
        {
            eventAggregator
                .GetEvent<NotificationEvents.ClearGroup>()
                .Publish(group);
        }

        public string Create(NotificationOptions options)
        {
            return Create(null, options);
        }

        public string Create(string notificationId, NotificationOptions options)
        {
            var notification = notificationFactory.Create(notificationId, options);

            eventAggregator
                .GetEvent<NotificationEvents.AddNotification>()
                .Publish(notification);

            return notification.NotificationId;
        }

        private void OnClosed(NotificationEvents.CloseNotificationArgs closeNotificationArgs)
        {
            var onClosed = Closed;
            if (onClosed != null)
            {
                var args = new CloseNotificationEventArgs(
                    closeNotificationArgs.Notification.NotificationId,
                    closeNotificationArgs.RemovedBy == RemovedBy.User);

                onClosed(this, args);
            }
        }

        private void OnClicked(NotificationEvents.ClickNotificationArgs clickNotificationArgs)
        {
            // only route events raised by clicking on main body
            if (clickNotificationArgs.ByCloseButton)
            {
                return;
            }

            var onClicked = Clicked;
            if (onClicked != null)
            {
                var args = new ClickNotificationEventArgs(
                    clickNotificationArgs.Notification.NotificationId);

                onClicked(this, args);
            }
        }
    }
}