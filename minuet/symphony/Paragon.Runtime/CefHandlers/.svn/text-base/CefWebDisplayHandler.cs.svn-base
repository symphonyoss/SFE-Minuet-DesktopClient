using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal sealed class CefWebDisplayHandler : CefDisplayHandler
    {
        private readonly ICefWebBrowserInternal _core;

        public CefWebDisplayHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override void OnTitleChange(CefBrowser browser, string title)
        {
            _core.OnTitleChanged(new TitleChangedEventArgs(title));
        }
    }
}