using System;
using System.Windows.Media;
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications.Models
{
    public class NotificationFactory : INotificationFactory
    {
        private readonly Func<Notification> createNotification;

        public NotificationFactory(Func<Notification> createNotification)
        {
            this.createNotification = createNotification;
        }

        public Notification Create(NotificationOptions options)
        {
            return Create(null, options);
        }

        public Notification Create(string notificationId, NotificationOptions options)
        {
            var notification = createNotification();
            notification.BackgroundColor = ConvertToColor(options.BackgroundColor);
            notification.BlinkColor = ConvertToColor(options.BlinkColor);
            notification.CanBlink = options.CanBlink;
            notification.CanPlaySound = options.CanPlaySound;
            notification.Group = options.Group;
            notification.IconUrl = options.IconUrl;
            notification.IsClickable = options.IsClickable;
            notification.IsPersistent = options.IsPersistent;
            notification.Message = options.Message;
            notification.NotificationId = GetId(notificationId);
            notification.SoundFile = options.SoundFile;
            notification.Title = options.Title;

            return notification;
        }

        private static string GetId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Guid.NewGuid().ToString();
            }

            return id;
        }

        private static SolidColorBrush ConvertToColor(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            if (!text.StartsWith("#"))
            {
                text = "#AA" + text;
            }

            var converter = new BrushConverter();
            var brush = converter.ConvertFromString(text) as SolidColorBrush;

            if (brush == null)
            {
                throw new NotSupportedException("Invalid color code: " + text);
            }

            return brush;
        }
    }
}