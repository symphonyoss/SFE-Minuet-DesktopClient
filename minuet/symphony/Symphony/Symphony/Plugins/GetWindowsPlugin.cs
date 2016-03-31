using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using System.Windows;
using System;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony", IsBrowserSide = true)]
    public class GetWindowsPlugin : IParagonPlugin
    {
        private IApplication application;

        public void Initialize(IApplication application)
        {
            this.application = application;
        }

        public void Shutdown()
        {
        }

        [JavaScriptPluginMember(Name = "cefGetScreenMedia")]
        public void cefGetScreenMedia(JavaScriptPluginCallback callback)
        {
            var mainWindow = (Window)this.application.WindowManager.AllWindows[0];

            var showDialog = new Action(() =>
            {
                var window = new MediaStreamPicker.MediaStreamPicker();
                window.Owner = mainWindow;
                window.ShowDialog();

                string stream = window.getSelectedMediaStream();

                callback(stream);
            });

            if (!mainWindow.Dispatcher.CheckAccess())
            {
                mainWindow.Dispatcher.Invoke(showDialog);
            }
            else
            {
                showDialog.Invoke();
            }
        }
    }
}
