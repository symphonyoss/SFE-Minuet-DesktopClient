using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications
{
    public class NotificationWindowManager : INotificationWindowManager
    {
        private readonly IViewModelFactory viewModelFactory;

        private NotificationWindowViewModel notificationWindow;

        UserConfigurationWindowViewModel configurationWindow = null;

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
            if (configurationWindow == null)
            {
                configurationWindow = viewModelFactory.CreateUserConfigurationWindow(owner);
                configurationWindow.RequestClose += configurationWindow_RequestClose;
            }
            configurationWindow.Show();

            return configurationWindow;
        }

        void configurationWindow_RequestClose(object sender, System.EventArgs e)
        {
            if (configurationWindow != null)
            {
                configurationWindow.RequestClose -= configurationWindow_RequestClose;
                configurationWindow = null;
            }
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