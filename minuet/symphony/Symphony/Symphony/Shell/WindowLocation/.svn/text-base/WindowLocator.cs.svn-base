//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Windows;
//using Paragon.Plugins;

//namespace Symphony.Shell
//{
//    public class WindowLocator : IWindowLocator
//    {
//        private const string RootName = "main";
//        private readonly IDictionary<string, BrowserWindow> windowCache;
//        private readonly IWindowFactory windowFactory;

//        private IApplication application;
        
//        public WindowLocator(IWindowFactory windowFactory)
//        {
//            this.windowFactory = windowFactory;
//            this.windowCache = new Dictionary<string, BrowserWindow>();
//        }

//        public IEnumerable<BrowserWindow> All()
//        {
//            return this.windowCache.Values.ToList();
//        }

//        public void AttachTo(IApplication application)
//        {
//            this.application = application;
//            this.application.Browser.ShowPopup += this.OnShowPopUp;

//            var window = this.application.Window.Unwrap();
//            window.Closed += this.OnRootWindowClosed;

//            var browserWindow = new BrowserWindow(RootName, application.Browser, application.Window);

//            this.windowCache.Add(browserWindow.Name, browserWindow);
//        }

//        public bool Remove(string name)
//        {
//            return this.windowCache.Remove(name);
//        }

//        public bool TryGetWindow(string name, out IWindow window)
//        {
//            BrowserWindow browserWindow;

//            var found = this.windowCache.TryGetValue(name, out browserWindow);

//            if (found)
//            {
//                window = browserWindow.Window;
//                return true;
//            }
            
//            window = null;
//            return false;
//        }

//        private void OnShowPopUp(object sender, ShowPopupEventArgs args)
//        {
//            var browser = args.PopupBrowser;
//            browser.ShowPopup += this.OnShowPopUp;

//            var window = this.windowFactory.GetWindow(browser);
//            var key = browser.BrowserName;

//            var browserWindow = new BrowserWindow(key, browser, window);
//            this.windowCache.Add(browserWindow.Name, browserWindow);
//        }

//        private void OnRootWindowClosed(object sender, EventArgs args)
//        {
//            var window = sender as Window;

//            if (window != null)
//            {
//                window.Closed -= this.OnRootWindowClosed;
//            }

//            foreach (var pair in this.windowCache)
//            {
//                pair.Value.Browser.ShowPopup -= this.OnShowPopUp;
//            }
//        }

//        public class BrowserWindow
//        {
//            public BrowserWindow(
//                string name,
//                ICefWebBrowser browser,
//                IWindow window)
//            {
//                this.Name = name;
//                this.Browser = browser;
//                this.Window = window;
//            }

//            public string Name { get; private set; }
//            public ICefWebBrowser Browser { get; private set; }
//            public IWindow Window { get; private set; }
//        }
//    }
//}
