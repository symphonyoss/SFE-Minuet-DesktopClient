using System;

namespace Paragon.Runtime
{
    public class TakeFocusEventArgs : EventArgs
    {
        public TakeFocusEventArgs(bool toExternalWindow = false)
        {
            ToExternalWindow = toExternalWindow;
        }

        /// <summary>
        /// This is to indicate that the browser lost focus to an external window.
        /// This is a short-term fix for onBlur event not being fired.
        /// </summary>
        public bool ToExternalWindow { get; private set; }
    }
}