namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// Callback interface used for asynchronous continuation of authentication
    /// requests.
    /// </summary>
    public sealed unsafe partial class CefRequestCallback
    {
        /// <summary>
        /// Continue the authentication request.
        /// </summary>
        public void Continue(bool allow)
        {
             cef_request_callback_t.cont(_self, allow ? 1 : 0);
        }

        /// <summary>
        /// Cancel the authentication request.
        /// </summary>
        public void Cancel()
        {
            cef_request_callback_t.cancel(_self);
        }
    }
}
