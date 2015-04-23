using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebJSDialogHandler : CefJSDialogHandler
    {
        private readonly ICefWebBrowserInternal _core;

        public CefWebJSDialogHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override bool OnJSDialog(CefBrowser browser, string originUrl, string acceptLang, CefJSDialogType dialogType, string messageText, string defaultPromptText, CefJSDialogCallback callback, out bool suppressMessage)
        {
            var ea = new JsDialogEventArgs(dialogType, messageText, defaultPromptText, callback);
            _core.OnJSDialog(ea);
            suppressMessage = ea.Handled && ea.SuppressMessage;
            return ea.Handled;
        }

        protected override bool OnBeforeUnloadDialog(CefBrowser browser, string messageText, bool isReload, CefJSDialogCallback callback)
        {
            var ea = new UnloadDialogEventArgs(messageText, callback);
            _core.OnBeforeUnloadDialog(ea);
            return ea.Handled;
        }

        protected override void OnResetDialogState(CefBrowser browser)
        {
            // Required method override.
        }

        protected override void OnDialogClosed(CefBrowser browser)
        {
            // Required method override.
        }
    }
}