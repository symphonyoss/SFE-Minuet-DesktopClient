namespace Symphony.Mvvm
{
    public interface IEventAggregator
    {
        T GetEvent<T>() where T : EventBase, new();
    }
}