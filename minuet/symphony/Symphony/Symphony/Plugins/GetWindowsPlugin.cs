using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using System.Windows;
using System;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "cef", IsBrowserSide = true)]
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

        [JavaScriptPluginMember(Name = "getScreenMedia")]
        public void cefGetScreenMedia(JavaScriptPluginCallback callback)
        {
            var mainWindow = (Window)this.application.WindowManager.AllWindows[0];

            var showDialog = new Action(() =>
            {
                var window = new MediaStreamPicker.MediaStreamPicker();
                window.Owner = mainWindow;
                window.ShowDialog();

                string stream = window.getSelectedMediaStream();

                if (String.IsNullOrEmpty(stream))
                    callback("media stream selected is null/empty", stream);
                else
                    callback(null, stream);
            });

            if (!mainWindow.Dispatcher.CheckAccess())
                mainWindow.Dispatcher.Invoke(showDialog);
            else
                showDialog.Invoke();
        }
    }
}
