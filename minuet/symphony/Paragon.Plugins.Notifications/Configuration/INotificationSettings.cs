namespace Paragon.Plugins.Notifications.Configuration
{
    public interface INotificationSettings
    {
        int GetSelectedMonitor();
        Position GetPosition();
        void Save(int selectedMonitor, Position position);
    }
}