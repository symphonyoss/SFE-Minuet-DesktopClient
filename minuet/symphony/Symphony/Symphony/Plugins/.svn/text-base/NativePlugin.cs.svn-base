using System;
using System.Windows;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using Symphony.Behaviors;
using Symphony.NativeServices;
using Symphony.Win32;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony.native", IsBrowserSide = true)]
    public class NativePlugin : IParagonPlugin
    {
        private CacheNativeService cacheNativeService;
        private ExternalNativeService externalNativeService;
        private SystemInfoNativeService systemInfoNativeService;

        public void Initialize(IApplication application)
        {
            var applicationLoadBehavior = new ApplicationLoadBehavior();
            applicationLoadBehavior.AttachTo(application);
            applicationLoadBehavior.Subscribe(applicationWindow =>
            {
                var downloadbehaviour = new DownloadBehavior();
                downloadbehaviour.AttachTo(applicationWindow);
            });

            this.cacheNativeService = new CacheNativeService();
            this.externalNativeService = new ExternalNativeService();
            this.systemInfoNativeService = new SystemInfoNativeService();
        }

        public void Shutdown()
        {
            
        }

        #region SystemInfo

        [JavaScriptPluginMember]
        public JObject GetClientInfoRequest()
        {
            return this
                .systemInfoNativeService
                .GetClientInfo();
        }

        #endregion

        #region External

        [JavaScriptPluginMember]
        public void OpenUrl(string url)
        {
            this.externalNativeService
                .OpenUrl(url);
        }


        [JavaScriptPluginMember]
        public void CallByKerberos(string kerberos)
        {
            this.externalNativeService
                .CallByKerberos(kerberos);
        }

        #endregion

        #region Cache

        [JavaScriptPluginMember]
        public JObject GetTempValue(string key, string callback)
        {
            return this
                .cacheNativeService
                .GetValue(key);
        }

        [JavaScriptPluginMember]
        public void SetTempValue(string key, string value)
        {
            this.cacheNativeService
                .SetValue(key, value);
        }

        #endregion

        #region Window

        [JavaScriptPluginMember]
        public bool GetWindowIsActive(IApplicationWindow applicationWindow)
        {
            var window = (Window)applicationWindow;

            return (bool)window.Dispatcher.Invoke(new Func<bool>(() =>
            {
                return window.IsActive;
            }));
        }

        [JavaScriptPluginMember]
        public void ShowWindowAndActivate(IApplicationWindow applicationWindow)
        {
            var window = (Window)applicationWindow;
            window.Dispatcher.Invoke(new Action(() =>
            {
                window.WindowState = WindowState.Normal;
                Win32Api.ShowWindow(applicationWindow.Handle);
            }));
        }

        [JavaScriptPluginMember]
        public void ShowWindowWithNoActivate(IApplicationWindow applicationWindow)
        {
            var window = (Window)applicationWindow;
            window.Dispatcher.Invoke(new Action(() =>
            {
                window.WindowState = WindowState.Normal;
                Win32Api.ShowWithNoActivate(applicationWindow.Handle, handler => window.Loaded += handler);
                window.Show();
            }));
        }

        #endregion
    }
}
