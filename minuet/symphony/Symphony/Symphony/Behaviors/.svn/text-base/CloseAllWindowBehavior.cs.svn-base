using System;
using System.Linq;
using System.Windows;
using Paragon.Plugins;

namespace Symphony.Behaviors
{
    public class CloseAllWindowBehavior
    {
        private IApplication application;
        private IApplicationWindow applicationWindow;
        private Window window;

        public void AttachTo(IApplication application, IApplicationWindow applicationWindow)
        {
            this.application = application;
            this.applicationWindow = applicationWindow;
            
            this.window = (Window) applicationWindow;
            this.window.Closing += this.OnMainWindowClosing;
        }

        private void OnMainWindowClosing(object sender, EventArgs args)
        {
            this.window.Closing -= this.OnMainWindowClosing;

            var allWindows = this.application
                .WindowManager
                .AllWindows
                .Where(win => win != this.applicationWindow)
                .ToArray();

            foreach (var child in allWindows)
            {
                child.CloseWindow();
            }
        }
    }
}