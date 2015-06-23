namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// A request context provides request handling for a set of related browser
    /// or URL request objects. A request context can be specified when creating a
    /// new browser via the CefBrowserHost static factory methods or when creating a
    /// new URL request via the CefURLRequest static factory methods. Browser objects
    /// with different request contexts will never be hosted in the same render
    /// process. Browser objects with the same request context may or may not be
    /// hosted in the same render process depending on the process model. Browser
    /// objects created indirectly via the JavaScript window.open function or
    /// targeted links will share the same render process and the same request
    /// context as the source browser. When running in single-process mode there is
    /// only a single render process (the main process) and so all browsers created
    /// in single-process mode will share the same request context. This will be the
    /// first request context passed into a CefBrowserHost static factory method and
    /// all other request context objects will be ignored.
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
        /// Creates a new context object with the specified |settings| and optional
        /// |handler|.
        /// </summary>
        public static cef_request_context_t* CreateContext(cef_request_context_settings_t* settings, cef_request_context_handler_t* handler)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.CreateContext
        }
        
        /// <summary>
        /// Creates a new context object that shares storage with |other| and uses an
        /// optional |handler|.
        /// </summary>
        public static cef_request_context_t* CreateContext(cef_request_context_t* other, cef_request_context_handler_t* handler)
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
        /// Returns true if this object is sharing the same storage as |that| object.
        /// </summary>
        public int IsSharingWith(cef_request_context_t* other)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.IsSharingWith
        }
        
        /// <summary>
        /// Returns true if this object is the global context. The global context is
        /// used by default when creating a browser or URL request with a NULL context
        /// argument.
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
        
        /// <summary>
        /// Returns the cache path for this object. If empty an "incognito mode"
        /// in-memory cache is being used.
        /// </summary>
        public cef_string_userfree* GetCachePath()
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.GetCachePath
        }
        
        /// <summary>
        /// Returns the default cookie manager for this object. This will be the global
        /// cookie manager if this object is the global request context. Otherwise,
        /// this will be the default cookie manager used when this request context does
        /// not receive a value via CefRequestContextHandler::GetCookieManager(). If
        /// |callback| is non-NULL it will be executed asnychronously on the IO thread
        /// after the manager's storage has been initialized.
        /// </summary>
        public cef_cookie_manager_t* GetDefaultCookieManager(cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.GetDefaultCookieManager
        }
        
        /// <summary>
        /// Register a scheme handler factory for the specified |scheme_name| and
        /// optional |domain_name|. An empty |domain_name| value for a standard scheme
        /// will cause the factory to match all domain names. The |domain_name| value
        /// will be ignored for non-standard schemes. If |scheme_name| is a built-in
        /// scheme and no handler is returned by |factory| then the built-in scheme
        /// handler factory will be called. If |scheme_name| is a custom scheme then
        /// you must also implement the CefApp::OnRegisterCustomSchemes() method in all
        /// processes. This function may be called multiple times to change or remove
        /// the factory that matches the specified |scheme_name| and optional
        /// |domain_name|. Returns false if an error occurs. This function may be
        /// called on any thread in the browser process.
        /// </summary>
        public int RegisterSchemeHandlerFactory(cef_string_t* scheme_name, cef_string_t* domain_name, cef_scheme_handler_factory_t* factory)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.RegisterSchemeHandlerFactory
        }
        
        /// <summary>
        /// Clear all registered scheme handler factories. Returns false on error. This
        /// function may be called on any thread in the browser process.
        /// </summary>
        public int ClearSchemeHandlerFactories()
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.ClearSchemeHandlerFactories
        }
        
    }
}
