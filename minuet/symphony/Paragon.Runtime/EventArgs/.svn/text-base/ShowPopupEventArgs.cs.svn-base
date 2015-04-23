using System;

namespace Paragon.Runtime
{
    public class ShowPopupEventArgs : EventArgs
    {
        public ShowPopupEventArgs(ICefWebBrowser browser)
        {
            PopupBrowser = browser;
            Shown = false;
        }

        /// <summary>
        /// The new popup browser control. The handler must show this in a custom frame window.
        /// </summary>
        public ICefWebBrowser PopupBrowser { get; private set; }

        /// <summary>
        /// The handler must set this property true. If this property is not set to true, the browser control will be immediately destroyed.
        /// </summary>
        public bool Shown { get; set; }
    }
}