using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebFocusHandler : CefFocusHandler
    {
        private readonly ICefWebBrowserInternal _core;

        public CefWebFocusHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override void OnTakeFocus(CefBrowser browser, bool next)
        {
            if (_core != null)
            {
                _core.OnTakeFocus(new TakeFocusEventArgs());
            }
        }
    }
}