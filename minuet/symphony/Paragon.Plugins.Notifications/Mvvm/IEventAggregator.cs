namespace Paragon.Plugins.Notifications.Mvvm
{
    public interface IEventAggregator
    {
        T GetEvent<T>() where T : EventBase, new();
    }
}