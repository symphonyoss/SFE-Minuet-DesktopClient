using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class RenderProcessTerminatedEventArgs : EventArgs
    {
        public RenderProcessTerminatedEventArgs(CefTerminationStatus status)
        {
            Status = status;
        }

        public CefTerminationStatus Status { get; private set; }
    }
}