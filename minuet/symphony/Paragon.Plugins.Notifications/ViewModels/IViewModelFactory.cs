namespace Paragon.Plugins.Notifications.ViewModels
{
    public interface IViewModelFactory
    {
        NotificationWindowViewModel CreateNotificationWindow();
        UserConfigurationWindowViewModel CreateUserConfigurationWindow(IApplicationWindow owner);
    }
}