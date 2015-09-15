using System;
using Paragon.Runtime.Win32;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal sealed class CefWebLifeSpanHandler : CefLifeSpanHandler
    {
        private static readonly object Lock = new object();
        private ICefWebBrowserInternal _core;

        public CefWebLifeSpanHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override void OnAfterCreated(CefBrowser browser)
        {
            base.OnAfterCreated(browser);

            if (_core != null)
            {
                _core.OnBrowserAfterCreated(browser);
            }
        }

        protected override bool DoClose(CefBrowser browser)
        {
            lock (Lock)
            {
                if (_core != null)
                {
                    Win32Api.SetParent(browser.GetHost().GetWindowHandle(), IntPtr.Zero);
                    OnBeforeClose(browser);
                }

                return false;
            }
        }

        protected override void OnBeforeClose(CefBrowser browser)
        {
            lock (Lock)
            {
                if (_core != null)
                {
                    var core = _core;
                    _core = null;
                    core.OnClosed(browser);
                }
            }
        }

        protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl,
            string targetFrameName, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo,
            ref CefClient client, CefBrowserSettings settings, ref bool noJavascriptAccess)
        {
            var e = new BeforePopupEventArgs(targetUrl, targetFrameName,
                popupFeatures, windowInfo, client, noJavascriptAccess);

            _core.OnBeforePopup(e);
            client = e.Client;
            noJavascriptAccess = e.NoJavascriptAccess;
            return e.Cancel;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _core = null;
            }

            base.Dispose(disposing);
        }
    }
}