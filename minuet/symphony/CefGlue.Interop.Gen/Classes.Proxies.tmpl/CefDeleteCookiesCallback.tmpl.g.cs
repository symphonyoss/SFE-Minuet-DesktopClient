namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Interface to implement to be notified of asynchronous completion via
    /// CefCookieManager::DeleteCookies().
    /// </summary>
    public sealed unsafe partial class CefDeleteCookiesCallback
    {
        /// <summary>
        /// Method that will be called upon completion. |num_deleted| will be the
        /// number of cookies that were deleted or -1 if unknown.
        /// </summary>
        public void OnComplete(int num_deleted)
        {
            throw new NotImplementedException(); // TODO: CefDeleteCookiesCallback.OnComplete
        }
        
    }
}
