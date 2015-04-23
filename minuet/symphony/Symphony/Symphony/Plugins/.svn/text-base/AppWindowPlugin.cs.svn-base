using System;
using System.Linq;
using System.Windows;
using Paragon.Plugins;
using Symphony.Behaviors;
using Symphony.Win32;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony.app.window", IsBrowserSide = true)]
    public class AppWindowPlugin : IParagonPlugin
    {
        private IApplication application;
        private CloseAllWindowBehavior closeAllWindowBehavior;

        public void Initialize(IApplication application)
        {
            this.application = application;

            this.closeAllWindowBehavior = new CloseAllWindowBehavior();

            var applicationLoadBehavior = new ApplicationLoadBehavior();
            applicationLoadBehavior.AttachTo(application);
            applicationLoadBehavior.Subscribe(
                applicationWindow =>
                {
                    this.closeAllWindowBehavior.AttachTo(application, applicationWindow);
                });
        }

        public void Shutdown()
        {
            
        }

        [JavaScriptPluginMember]
        public bool IsWindowActive(string name)
        {
            IApplicationWindow applicationWindow;
            var window = this.GetWindowByName(name, out applicationWindow);
            
            return (bool)window.Dispatcher.Invoke(new Func<bool>(() =>
            {
                return window.IsActive;
            }));
        }

        [JavaScriptPluginMember]
        public void ShowWindow(string name)
        {
            IApplicationWindow applicationWindow;
            var window = this.GetWindowByName(name, out applicationWindow);

            window.Dispatcher.Invoke(new Action(() =>
            {
                Win32Api.ShowWindow(applicationWindow.Handle);
            }));
        }

        [JavaScriptPluginMember]
        public void ShowWindowWithNoActivate(string name)
        {
            IApplicationWindow applicationWindow;
            var window = this.GetWindowByName(name, out applicationWindow);

            window.Dispatcher.Invoke(new Action(() =>
            {
                window.WindowState = WindowState.Normal;
                Win32Api.ShowWithNoActivate(applicationWindow.Handle, handler => window.Loaded += handler);
                window.Show();
            }));
        }

        private Window GetWindowByName(string name, out IApplicationWindow applicationWindow)
        {
            applicationWindow = this.application
                .WindowManager
                .AllWindows.
                FirstOrDefault(win => win.GetId() == name);

            return applicationWindow as Window;
        }


    }
}
