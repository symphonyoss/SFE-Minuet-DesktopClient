using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class RenderProcessTerminatedEventArgs : EventArgs
    {
        public RenderProcessTerminatedEventArgs(CefBrowser browser, CefTerminationStatus status)
        {
            Browser = browser;
            Status = status;
        }

        internal CefBrowser Browser { get; private set; }
        public CefTerminationStatus Status { get; private set; }
    }
}