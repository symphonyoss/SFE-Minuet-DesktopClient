using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Paragon.Plugins;
using Symphony.Configuration;
using Symphony.Mvvm;
using Symphony.Shell.HotKeys;
using Symphony.Shell.SystemMenu.MenuItems;

namespace Symphony.Shell.SystemMenu
{
    public class SystemMenuExtension : Extension
    {
        private readonly IUnityContainer container;

        public SystemMenuExtension(IUnityContainer container)
            : base(container)
        {
            this.container = container;
        }

        public override void Initalize(IApplication application)
        {
            base.Initalize(application);

            var window = application.FindWindow(PluginExecutionContext.BrowserIdentifier);

            if (window != null)
            {
                var refresh = new Refresh(window);

                var minimizeOnClose = new MinimizeOnClose(
                    window, 
                    this.container.Resolve<ApplicationSettings>());

                var editHotKey = new EditHotKey(
                    window, 
                    this.container.Resolve<HotKeySettings>(),
                    this.container.Resolve<IEventAggregator>());

                var forceClose = new ForceClose(window);
                forceClose.BeforeClose += (sender, args) => minimizeOnClose.IsChecked = false;

                var version = new SymphonyVersion();

                var menuItems = new List<SystemMenuItem>();
                menuItems.Add(refresh);
                menuItems.Add(minimizeOnClose);
                menuItems.Add(editHotKey);
                menuItems.Add(forceClose);
                menuItems.Add(version);

                var systemMenuWindow = new SystemMenuInterceptor(menuItems);
                systemMenuWindow.ApplyToWindow(window);
            }
        }
    }
}
