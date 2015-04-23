using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal sealed class CefWebContextMenuHandler : CefContextMenuHandler
    {
        private readonly ICefWebBrowserInternal _core;

        public CefWebContextMenuHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams state, CefMenuModel model)
        {
            _core.OnBeforeContextMenu(new ContextMenuEventArgs(state, model));
        }

        protected override bool OnContextMenuCommand(CefBrowser browser, CefFrame frame, CefContextMenuParams state, int commandId, CefEventFlags eventFlags)
        {
            var ea = new ContextMenuCommandEventArgs(state, commandId);
            _core.OnContextMenuCommand(ea);
            return ea.Handled;
        }
    }
}