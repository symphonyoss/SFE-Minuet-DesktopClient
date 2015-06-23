namespace Paragon.Plugins.Notifications.Configuration
{
    public interface INotificationSettings
    {
        int GetSelectedMonitor();
        Position GetPosition();
        bool GetDnd();
        void Save(int selectedMonitor, Position position, bool dnd);
    }
}