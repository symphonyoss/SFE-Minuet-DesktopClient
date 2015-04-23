using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class UnloadDialogEventArgs : EventArgs
    {
        internal UnloadDialogEventArgs(string messageText, CefJSDialogCallback callback)
        {
            MessageText = messageText;
            Callback = callback;
        }

        public string MessageText { get; private set; }
        public CefJSDialogCallback Callback { get; private set; }
        public bool Handled { get; set; }
    }
}