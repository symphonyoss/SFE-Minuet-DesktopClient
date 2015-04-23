using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications
{
    public class NotificationWindowManager : INotificationWindowManager
    {
        private readonly IViewModelFactory viewModelFactory;

        private NotificationWindowViewModel notificationWindow;

        public NotificationWindowManager(IViewModelFactory viewModelFactory)
        {
            this.viewModelFactory = viewModelFactory;
        }

        public NotificationWindowViewModel ShowNotificationWindow()
        {
            return notificationWindow ?? (notificationWindow = viewModelFactory.CreateNotificationWindow());
        }

        public UserConfigurationWindowViewModel ShowSettings(IApplicationWindow owner)
        {
            var configurationWindow = viewModelFactory.CreateUserConfigurationWindow(owner);
            configurationWindow.Show();

            return configurationWindow;
        }

        public void Shutdown()
        {
            if (notificationWindow != null)
            {
                notificationWindow.Close();
            }
        }
    }
}