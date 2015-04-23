using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal sealed class CefWebLoadHandler : CefLoadHandler
    {
        private readonly ICefWebBrowserInternal _core;

        public CefWebLoadHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
        {
            _core.OnLoadStart(new LoadStartEventArgs(frame));
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            _core.OnLoadEnd(new LoadEndEventArgs(frame));
        }

        protected override void OnLoadError(CefBrowser browser, CefFrame frame, CefErrorCode errorCode, string errorText, string failedUrl)
        {
            _core.OnLoadError(new LoadErrorEventArgs(frame, errorCode, errorText, failedUrl));
        }
    }
}