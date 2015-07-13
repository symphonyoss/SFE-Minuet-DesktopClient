using System;
using System.Windows.Media;
using Paragon.Plugins.Notifications.Contracts;
using Paragon.Runtime;

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
            SolidColorBrush fallback = new SolidColorBrush(Colors.Black);

            if (string.IsNullOrEmpty(text))
                return fallback;

            if (!text.StartsWith("#"))
                text = "#" + text;

            SolidColorBrush brush = null;

            var converter = new BrushConverter();
            if (converter.CanConvertFrom(typeof (string)))
            {
                try
                {
                    brush = (SolidColorBrush)converter.ConvertFrom(text);
                } 
                catch (Exception e)
                {
                    ILogger Logger = ParagonLogManager.GetLogger();
                    Logger.Error("error ConvertFrom when converting color:" + text + ", exception: " + e.ToString());

                    // fallback in case of exception
                    brush = fallback;
                }
            }

            if (brush == null)
                brush = fallback;

            return brush;
        }
    }
}