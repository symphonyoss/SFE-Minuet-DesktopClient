namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Interface to implement to be notified of asynchronous completion via
    /// CefCookieManager::SetCookie().
    /// </summary>
    public sealed unsafe partial class CefSetCookieCallback
    {
        /// <summary>
        /// Method that will be called upon completion. |success| will be true if the
        /// cookie was set successfully.
        /// </summary>
        public void OnComplete(int success)
        {
            throw new NotImplementedException(); // TODO: CefSetCookieCallback.OnComplete
        }
        
    }
}
