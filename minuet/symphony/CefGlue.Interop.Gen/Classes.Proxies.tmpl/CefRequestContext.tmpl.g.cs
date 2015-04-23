namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// A request context provides request handling for a set of related browser
    /// objects. A request context is specified when creating a new browser object
    /// via the CefBrowserHost static factory methods. Browser objects with different
    /// request contexts will never be hosted in the same render process. Browser
    /// objects with the same request context may or may not be hosted in the same
    /// render process depending on the process model. Browser objects created
    /// indirectly via the JavaScript window.open function or targeted links will
    /// share the same render process and the same request context as the source
    /// browser. When running in single-process mode there is only a single render
    /// process (the main process) and so all browsers created in single-process mode
    /// will share the same request context. This will be the first request context
    /// passed into a CefBrowserHost static factory method and all other request
    /// context objects will be ignored.
    /// </summary>
    public sealed unsafe partial class CefRequestContext
    {
        /// <summary>
        /// Returns the global context object.
        /// </summary>
        public static cef_request_context_t* GetGlobalContext()
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.GetGlobalContext
        }
        
        /// <summary>
        /// Creates a new context object with the specified handler.
        /// </summary>
        public static cef_request_context_t* CreateContext(cef_request_context_handler_t* handler)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.CreateContext
        }
        
        /// <summary>
        /// Returns true if this object is pointing to the same context as |that|
        /// object.
        /// </summary>
        public int IsSame(cef_request_context_t* other)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.IsSame
        }
        
        /// <summary>
        /// Returns true if this object is the global context.
        /// </summary>
        public int IsGlobal()
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.IsGlobal
        }
        
        /// <summary>
        /// Returns the handler for this context if any.
        /// </summary>
        public cef_request_context_handler_t* GetHandler()
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.GetHandler
        }
        
    }
}
