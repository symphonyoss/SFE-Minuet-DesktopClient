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
        
        /// <summary>
        /// Tells all renderer processes associated with this context to throw away
        /// their plugin list cache. If |reload_pages| is true they will also reload
        /// all pages with plugins. CefRequestContextHandler::OnBeforePluginLoad may
        /// be called to rebuild the plugin list cache.
        /// </summary>
        public void PurgePluginListCache(int reload_pages)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.PurgePluginListCache
        }
        
        /// <summary>
        /// Returns true if a preference with the specified |name| exists. This method
        /// must be called on the browser process UI thread.
        /// </summary>
        public int HasPreference(cef_string_t* name)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.HasPreference
        }
        
        /// <summary>
        /// Returns the value for the preference with the specified |name|. Returns
        /// NULL if the preference does not exist. The returned object contains a copy
        /// of the underlying preference value and modifications to the returned object
        /// will not modify the underlying preference value. This method must be called
        /// on the browser process UI thread.
        /// </summary>
        public cef_value_t* GetPreference(cef_string_t* name)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.GetPreference
        }
        
        /// <summary>
        /// Returns all preferences as a dictionary. If |include_defaults| is true then
        /// preferences currently at their default value will be included. The returned
        /// object contains a copy of the underlying preference values and
        /// modifications to the returned object will not modify the underlying
        /// preference values. This method must be called on the browser process UI
        /// thread.
        /// </summary>
        public cef_dictionary_value_t* GetAllPreferences(int include_defaults)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.GetAllPreferences
        }
        
        /// <summary>
        /// Returns true if the preference with the specified |name| can be modified
        /// using SetPreference. As one example preferences set via the command-line
        /// usually cannot be modified. This method must be called on the browser
        /// process UI thread.
        /// </summary>
        public int CanSetPreference(cef_string_t* name)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.CanSetPreference
        }
        
        /// <summary>
        /// Set the |value| associated with preference |name|. Returns true if the
        /// value is set successfully and false otherwise. If |value| is NULL the
        /// preference will be restored to its default value. If setting the preference
        /// fails then |error| will be populated with a detailed description of the
        /// problem. This method must be called on the browser process UI thread.
        /// </summary>
        public int SetPreference(cef_string_t* name, cef_value_t* value, cef_string_t* error)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.SetPreference
        }
        
        /// <summary>
        /// Clears all certificate exceptions that were added as part of handling
        /// CefRequestHandler::OnCertificateError(). If you call this it is
        /// recommended that you also call CloseAllConnections() or you risk not
        /// being prompted again for server certificates if you reconnect quickly.
        /// If |callback| is non-NULL it will be executed on the UI thread after
        /// completion.
        /// </summary>
        public void ClearCertificateExceptions(cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.ClearCertificateExceptions
        }
        
        /// <summary>
        /// Clears all active and idle connections that Chromium currently has.
        /// This is only recommended if you have released all other CEF objects but
        /// don't yet want to call CefShutdown(). If |callback| is non-NULL it will be
        /// executed on the UI thread after completion.
        /// </summary>
        public void CloseAllConnections(cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.CloseAllConnections
        }
        
        /// <summary>
        /// Attempts to resolve |origin| to a list of associated IP addresses.
        /// |callback| will be executed on the UI thread after completion.
        /// </summary>
        public void ResolveHost(cef_string_t* origin, cef_resolve_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.ResolveHost
        }
        
        /// <summary>
        /// Attempts to resolve |origin| to a list of associated IP addresses using
        /// cached data. |resolved_ips| will be populated with the list of resolved IP
        /// addresses or empty if no cached data is available. Returns ERR_NONE on
        /// success. This method must be called on the browser process IO thread.
        /// </summary>
        public CefErrorCode ResolveHostCached(cef_string_t* origin, cef_string_list* resolved_ips)
        {
            throw new NotImplementedException(); // TODO: CefRequestContext.ResolveHostCached
        }
        
    }
}
