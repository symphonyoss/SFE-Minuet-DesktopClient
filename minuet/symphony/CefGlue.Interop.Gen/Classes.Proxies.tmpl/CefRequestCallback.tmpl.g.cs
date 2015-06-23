namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Callback interface used for asynchronous continuation of url requests.
    /// </summary>
    public sealed unsafe partial class CefRequestCallback
    {
        /// <summary>
        /// Continue the url request. If |allow| is true the request will be continued.
        /// Otherwise, the request will be canceled.
        /// </summary>
        public void Continue(int allow)
        {
            throw new NotImplementedException(); // TODO: CefRequestCallback.Continue
        }
        
        /// <summary>
        /// Cancel the url request.
        /// </summary>
        public void Cancel()
        {
            throw new NotImplementedException(); // TODO: CefRequestCallback.Cancel
        }
        
    }
}
