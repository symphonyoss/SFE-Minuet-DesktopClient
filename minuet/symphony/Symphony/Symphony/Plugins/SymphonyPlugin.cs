using System.Diagnostics;
using Microsoft.Practices.Unity;
using Paragon.Plugins;
using Symphony.Configuration;
using Symphony.Shell;
using Symphony.Shell.HotKeys;
using Symphony.Shell.IO;
using Symphony.Shell.SystemMenu;
using Symphony.Shell.WindowLocation;
using Symphony.Shell.WindowManagement;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony", IsBrowserSide = true)]
    public class SymphonyPlugin : IParagonPlugin
    {
        private readonly IUnityContainer container;
        
        public SymphonyPlugin()
        {
            Debugger.Break();
            this.container = new UnityContainer();
        }

        public void Initialize(IApplication application)
        {
            Debugger.Break();
            this.container
                .RegisterType<IExtension, ConfigurationExtension>("Extension_Configuration")
                //.RegisterType<IExtension, DiagnosticsExtension>("Extension_Diagnostics")
                //.RegisterType<IExtension, NotificationExtension>("Extension_Notifications")
                //.RegisterType<IExtension, ScreenCaptureExtension>("Extension_ScreenCapture")
                .RegisterType<IExtension, HotKeyExtension>("Extension_HotKeys")
                .RegisterType<IExtension, SystemMenuExtension>("Extension_SystemMenu")
                .RegisterType<IExtension, WindowExtension>("Extension_Windows")
                .RegisterType<IExtension, DownloadExtension>("Extension_Symphony_Download")
                .RegisterType<IExtension, WindowManagementExtension>("Extension_Symphony_WindowManagement");

            var extensions = this.container.ResolveAll<IExtension>();
            foreach (var extension in extensions)
            {
                extension.Initalize(application);
            }

            //application.Browser.DisableContextMenu = true;

            this.container
                .RegisterSingleton(application);

            //this.container
            //    .Resolve<IWindowLocator>()
            //    .AttachTo(application);

            this.container
                .Resolve<WindowInterceptor>()
                .AttachToApplication(application);
        }

        public void Shutdown()
        {
            var extensions = this.container.ResolveAll<IExtension>();
            foreach (var extension in extensions)
            {
                extension.Shutdown();
            }
        }

        [JavaScriptPluginMember(Name = "____stub")]
        public void Stub()
        {
            
        }
    }
}
