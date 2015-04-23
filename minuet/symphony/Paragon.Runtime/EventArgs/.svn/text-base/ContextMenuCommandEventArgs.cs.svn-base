using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public sealed class ContextMenuCommandEventArgs : EventArgs
    {
        public ContextMenuCommandEventArgs(CefContextMenuParams state, int commandId)
        {
            Command = commandId;
            State = state;
            Handled = false;
        }

        public CefContextMenuParams State { get; private set; }
        public int Command { get; private set; }
        public bool Handled { get; set; }
    }
}