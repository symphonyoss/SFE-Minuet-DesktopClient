namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Callback interface used for asynchronous continuation of quota requests.
    /// </summary>
    public sealed unsafe partial class CefQuotaCallback
    {
        /// <summary>
        /// Continue the quota request. If |allow| is true the request will be
        /// allowed. Otherwise, the request will be denied.
        /// </summary>
        public void Continue(int allow)
        {
            throw new NotImplementedException(); // TODO: CefQuotaCallback.Continue
        }
        
        /// <summary>
        /// Cancel the quota request.
        /// </summary>
        public void Cancel()
        {
            throw new NotImplementedException(); // TODO: CefQuotaCallback.Cancel
        }
        
    }
}
