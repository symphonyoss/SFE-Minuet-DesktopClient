using System.Windows;
using Paragon.Plugins;

namespace Paragon.Runtime.Kernel.Windowing
{
    public interface IApplicationWindowEx : IApplicationWindow
    {
        ICefWebBrowser Browser { get; }
        void Initialize(IApplicationWindowManagerEx windowManager, ICefWebBrowser browser, string startUrl, string title, CreateWindowOptions options);
        void ShowDeveloperTools(Point insepectElement);
    }
}