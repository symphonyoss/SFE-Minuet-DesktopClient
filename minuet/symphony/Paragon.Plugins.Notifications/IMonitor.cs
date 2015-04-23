using System.Windows;

namespace Paragon.Plugins.Notifications
{
    public interface IMonitor
    {
        Rect DeviceBounds { get; }
        Rect WorkingArea { get; }
        bool IsPrimary { get; }
        string DeviceName { get; }
    }
}