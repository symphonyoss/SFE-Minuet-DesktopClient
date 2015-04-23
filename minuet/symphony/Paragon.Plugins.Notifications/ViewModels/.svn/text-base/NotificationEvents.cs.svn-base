using Paragon.Plugins.Notifications.Models;
using Paragon.Plugins.Notifications.Mvvm;

namespace Paragon.Plugins.Notifications.ViewModels
{
    public class NotificationEvents
    {
        public class ActivateNotifciationCentre : PubSubEvent<EmptyArgs>
        {
        }

        public class AddNotification : PubSubEvent<Notification>
        {
        }

        public class AddNotificationArgs
        {
            public Notification Notification { get; set; }
            public Position Position { get; set; }
            public int MonitorIndex { get; set; }
        }

        public class AddNotificationWithPosition : PubSubEvent<AddNotificationArgs>
        {
        }

        public class Clear : PubSubEvent<ClearNotificationArgs>
        {
        }

        public class ClearAll : PubSubEvent<EmptyArgs>
        {
        }

        public class ClearGroup : PubSubEvent<string>
        {
        }

        public class ClearNotificationArgs
        {
            public string NotificationId { get; set; }
        }

        public class ClickNotification : PubSubEvent<ClickNotificationArgs>
        {
        }

        public class ClickNotificationArgs
        {
            public Notification Notification { get; set; }
            public bool ByCloseButton { get; set; }
        }

        public class CloseNotification : PubSubEvent<CloseNotificationArgs>
        {
        }

        public class CloseNotificationArgs
        {
            public Notification Notification { get; set; }
            public RemovedBy RemovedBy { get; set; }
        }

        public class RefreshNotificationCentre : PubSubEvent<EmptyArgs>
        {
        }
    }
}