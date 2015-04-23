using System;
using Paragon.Plugins;

namespace Paragon.Runtime.Kernel.Windowing
{
    public interface IApplicationWindowManagerEx : IApplicationWindowManager
    {
        void Initialize(IApplication application, Func<IApplicationWindowEx> createNewWindow, Func<ICefWebBrowser> getRootBrowser);
        void CreateWindow(CreateWindowRequest request);
        void BeforeApplicationWindowPopup(BeforePopupEventArgs eventArgs);
        void ShowApplicationWindowPopup(IApplicationWindowEx applicationWindow, ShowPopupEventArgs eventArgs);
        void RemoveApplicationWindow(IApplicationWindowEx applicationWindow);
        void Shutdown();
    }
}