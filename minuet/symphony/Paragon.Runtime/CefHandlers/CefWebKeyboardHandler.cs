using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal sealed class CefWebKeyboardHandler : CefKeyboardHandler
    {
        private readonly ICefWebBrowserInternal _core;

        public CefWebKeyboardHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override bool OnPreKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, IntPtr osEvent, out bool isKeyboardShortcut)
        {
            isKeyboardShortcut = false;
            _core.OnPreviewKeyEvent(keyEvent);
            return false;
        }
    }
}