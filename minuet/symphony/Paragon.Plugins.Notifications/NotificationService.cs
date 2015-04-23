using System;
using System.Windows;
using System.Windows.Threading;
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly Dispatcher dispatcher;
        private readonly Func<INotificationWindowManager> getNotificationWindowManager;

        public NotificationService(
            Func<INotificationWindowManager> getNotificationWindowManager)
            : this(getNotificationWindowManager, Application.Current.Dispatcher)
        {
        }

        public NotificationService(
            Func<INotificationWindowManager> getNotificationWindowManager,
            Dispatcher dispatcher)
        {
            this.getNotificationWindowManager = getNotificationWindowManager;
            this.dispatcher = dispatcher;
        }

        public void Start()
        {
            ShowNotificationWindow();
        }

        public void Shutdown()
        {
            ShutdownNotificationWindow();
        }

        private void ShowNotificationWindow()
        {
            var notificationWindowManager = getNotificationWindowManager();
            Action showAction = () => notificationWindowManager.ShowNotificationWindow();

            dispatcher.Invoke(showAction);
        }

        private void ShutdownNotificationWindow()
        {
            var notificationWindowManager = getNotificationWindowManager();
            Action shutdownAction = notificationWindowManager.Shutdown;

            dispatcher.Invoke(shutdownAction);
        }
    }
}