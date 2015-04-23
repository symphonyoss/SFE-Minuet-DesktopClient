using System.Windows.Threading;
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications.Client
{
    public class LocalNotificationService : INotificationService
    {
        private readonly NotificationService notificationService;

        public LocalNotificationService(
            LocalServiceFactory serviceFactory,
            Dispatcher dispatcher)
        {
            notificationService = new NotificationService(() => serviceFactory.NotificationWindowManager, dispatcher);
        }

        public void Start()
        {
            notificationService.Start();
        }

        public void Shutdown()
        {
            notificationService.Shutdown();
        }
    }
}