using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class JsDialogEventArgs : EventArgs
    {
        internal JsDialogEventArgs(CefJSDialogType dialogType, string messageText, string defaultPromptText, CefJSDialogCallback callback)
        {
            DialogType = dialogType;
            MessageText = messageText;
            DefaultPromptText = defaultPromptText;
            Callback = callback;
            SuppressMessage = false;
            Handled = false;
        }

        public CefJSDialogType DialogType { get; private set; }
        public string MessageText { get; private set; }
        public string DefaultPromptText { get; private set; }
        public CefJSDialogCallback Callback { get; private set; }
        public bool SuppressMessage { get; set; }
        public bool Handled { get; set; }
    }
}