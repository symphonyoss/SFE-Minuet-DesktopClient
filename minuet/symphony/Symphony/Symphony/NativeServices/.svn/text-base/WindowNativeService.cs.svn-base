using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Symphony.Shell;

namespace Symphony.NativeServices
{
    public class WindowNativeService
    {
        //private readonly IWindowLocator windowLocator;
        private const int MinWidth = 50;

        //public WindowNativeService(IWindowLocator windowLocator)
        //{
        //    this.windowLocator = windowLocator;
        //}

        public void Activate(string windowName, bool activate)
        {
            //IWindow window;

            //if (this.windowLocator.TryGetWindow(windowName, out window))
            //{
            //    window.WindowState = WindowState.Normal;

            //    if (!activate)
            //    {
            //        window.ShowWithNoActivate();
            //    }
            //    else
            //    {
            //        window.Show();
            //    }
            //}
        }

        public void Close(string windowName)
        {
            //IWindow window;

            //if (this.windowLocator.TryGetWindow(windowName, out window))
            //{
            //    window.Close();
                
            //    this.windowLocator.Remove(windowName);
            //}
        }

        public JObject GetActiveWindow()
        {
            //var active = this.windowLocator
            //    .All()
            //    .FirstOrDefault(i => i.Window.IsActive);

            var json = new JObject();

            //if (active != null)
            //{
            //    json["windowName"] = active.Name;
            //}

            return json;
        }

        public void SetMinWidth(string windowName, int minWidth)
        {
            if (minWidth < MinWidth)
            {
                Trace.TraceWarning("Min Width cannot be set to less than 50");
                return;
            }

            //BrowserContext context = WindowManager.Instance.Get(windowName);
            //if (context != null)
            {
                Trace.TraceInformation("Setting window minimum width: windowName={0}, minWidth={1}", windowName, minWidth);
                //context.Window.MinWidth = minWidth;
            }
        }
    }
}
