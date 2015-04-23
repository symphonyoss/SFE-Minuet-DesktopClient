using System;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public static class CefV8Extensions
    {
        public static int GetBrowserId(this CefV8Context context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            using (var browser = context.GetBrowser())
            {
                return browser.Identifier;
            }
        }

        public static CefBrowser Clone(this CefBrowser browser)
        {
            if (browser == null)
            {
                throw new ArgumentNullException("browser");
            }
            return browser.GetMainFrame().Browser;
        }
    }
}