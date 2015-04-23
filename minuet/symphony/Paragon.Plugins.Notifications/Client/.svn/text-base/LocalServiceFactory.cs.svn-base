using Paragon.Plugins.Notifications.Configuration;
using Paragon.Plugins.Notifications.Contracts;
using Paragon.Plugins.Notifications.Models;
using Paragon.Plugins.Notifications.Mvvm;
using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications.Client
{
    public class LocalServiceFactory
    {
        public LocalServiceFactory(INotificationSettings notificationSettings)
        {
            var monitors = new Monitors();

            EventAggregator = new EventAggregator();
            NotificationFactory = new NotificationFactory(() => new Notification(EventAggregator));

            var viewModelFactory = new ViewModelFactory(
                EventAggregator,
                () => new UserConfigurationWindowViewModel(
                    monitors,
                    notificationSettings,
                    EventAggregator,
                    NotificationFactory),
                () => new NotificationWindowViewModel(
                    monitors,
                    notificationSettings,
                    EventAggregator));

            NotificationWindowManager = new NotificationWindowManager(viewModelFactory);
            NotificationService = new NotificationService(() => NotificationWindowManager);
        }

        public IEventAggregator EventAggregator { get; private set; }

        public INotificationFactory NotificationFactory { get; private set; }

        public INotificationService NotificationService { get; private set; }

        public INotificationWindowManager NotificationWindowManager { get; private set; }
    }
}