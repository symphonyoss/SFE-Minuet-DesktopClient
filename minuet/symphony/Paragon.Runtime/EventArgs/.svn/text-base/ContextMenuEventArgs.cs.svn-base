using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public sealed class ContextMenuEventArgs : EventArgs
    {
        public ContextMenuEventArgs(CefContextMenuParams state, CefMenuModel model)
        {
            Model = model;
            State = state;
            Handled = false;
        }

        public CefContextMenuParams State { get; private set; }
        public CefMenuModel Model { get; private set; }
        public bool Handled { get; set; }
    }
}