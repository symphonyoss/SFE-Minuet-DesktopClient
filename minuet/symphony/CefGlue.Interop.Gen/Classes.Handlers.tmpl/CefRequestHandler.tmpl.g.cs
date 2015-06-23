namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to handle events related to browser requests. The
    /// methods of this class will be called on the thread indicated.
    /// </summary>
    public abstract unsafe partial class CefRequestHandler
    {
        private int on_before_browse(cef_request_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, int is_redirect)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnBeforeBrowse
        }
        
        /// <summary>
        /// Called on the UI thread before browser navigation. Return true to cancel
        /// the navigation or false to allow the navigation to proceed. The |request|
        /// object cannot be modified in this callback.
        /// CefLoadHandler::OnLoadingStateChange will be called twice in all cases.
        /// If the navigation is allowed CefLoadHandler::OnLoadStart and
        /// CefLoadHandler::OnLoadEnd will be called. If the navigation is canceled
        /// CefLoadHandler::OnLoadError will be called with an |errorCode| value of
        /// ERR_ABORTED.
        /// </summary>
        // protected abstract int OnBeforeBrowse(cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, int is_redirect);
        
        private int on_open_urlfrom_tab(cef_request_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, cef_string_t* target_url, CefWindowOpenDisposition target_disposition, int user_gesture)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnOpenURLFromTab
        }
        
        /// <summary>
        /// Called on the UI thread before OnBeforeBrowse in certain limited cases
        /// where navigating a new or different browser might be desirable. This
        /// includes user-initiated navigation that might open in a special way (e.g.
        /// links clicked via middle-click or ctrl + left-click) and certain types of
        /// cross-origin navigation initiated from the renderer process (e.g.
        /// navigating the top-level frame to/from a file URL). The |browser| and
        /// |frame| values represent the source of the navigation. The
        /// |target_disposition| value indicates where the user intended to navigate
        /// the browser based on standard Chromium behaviors (e.g. current tab,
        /// new tab, etc). The |user_gesture| value will be true if the browser
        /// navigated via explicit user gesture (e.g. clicking a link) or false if it
        /// navigated automatically (e.g. via the DomContentLoaded event). Return true
        /// to cancel the navigation or false to allow the navigation to proceed in the
        /// source browser's top-level frame.
        /// </summary>
        // protected abstract int OnOpenURLFromTab(cef_browser_t* browser, cef_frame_t* frame, cef_string_t* target_url, CefWindowOpenDisposition target_disposition, int user_gesture);
        
        private CefReturnValue on_before_resource_load(cef_request_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, cef_request_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnBeforeResourceLoad
        }
        
        /// <summary>
        /// Called on the IO thread before a resource request is loaded. The |request|
        /// object may be modified. Return RV_CONTINUE to continue the request
        /// immediately. Return RV_CONTINUE_ASYNC and call CefRequestCallback::
        /// Continue() at a later time to continue or cancel the request
        /// asynchronously. Return RV_CANCEL to cancel the request immediately.
        /// </summary>
        // protected abstract CefReturnValue OnBeforeResourceLoad(cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, cef_request_callback_t* callback);
        
        private cef_resource_handler_t* get_resource_handler(cef_request_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.GetResourceHandler
        }
        
        /// <summary>
        /// Called on the IO thread before a resource is loaded. To allow the resource
        /// to load normally return NULL. To specify a handler for the resource return
        /// a CefResourceHandler object. The |request| object should not be modified in
        /// this callback.
        /// </summary>
        // protected abstract cef_resource_handler_t* GetResourceHandler(cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request);
        
        private void on_resource_redirect(cef_request_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, cef_string_t* new_url)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnResourceRedirect
        }
        
        /// <summary>
        /// Called on the IO thread when a resource load is redirected. The |request|
        /// parameter will contain the old URL and other request-related information.
        /// The |new_url| parameter will contain the new URL and can be changed if
        /// desired. The |request| object cannot be modified in this callback.
        /// </summary>
        // protected abstract void OnResourceRedirect(cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, cef_string_t* new_url);
        
        private int on_resource_response(cef_request_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, cef_response_t* response)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnResourceResponse
        }
        
        /// <summary>
        /// Called on the IO thread when a resource response is received. To allow the
        /// resource to load normally return false. To redirect or retry the resource
        /// modify |request| (url, headers or post body) and return true. The
        /// |response| object cannot be modified in this callback.
        /// </summary>
        // protected abstract int OnResourceResponse(cef_browser_t* browser, cef_frame_t* frame, cef_request_t* request, cef_response_t* response);
        
        private int get_auth_credentials(cef_request_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, int isProxy, cef_string_t* host, int port, cef_string_t* realm, cef_string_t* scheme, cef_auth_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.GetAuthCredentials
        }
        
        /// <summary>
        /// Called on the IO thread when the browser needs credentials from the user.
        /// |isProxy| indicates whether the host is a proxy server. |host| contains the
        /// hostname and |port| contains the port number. Return true to continue the
        /// request and call CefAuthCallback::Continue() either in this method or
        /// at a later time when the authentication information is available. Return
        /// false to cancel the request immediately.
        /// </summary>
        // protected abstract int GetAuthCredentials(cef_browser_t* browser, cef_frame_t* frame, int isProxy, cef_string_t* host, int port, cef_string_t* realm, cef_string_t* scheme, cef_auth_callback_t* callback);
        
        private int on_quota_request(cef_request_handler_t* self, cef_browser_t* browser, cef_string_t* origin_url, long new_size, cef_request_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnQuotaRequest
        }
        
        /// <summary>
        /// Called on the IO thread when JavaScript requests a specific storage quota
        /// size via the webkitStorageInfo.requestQuota function. |origin_url| is the
        /// origin of the page making the request. |new_size| is the requested quota
        /// size in bytes. Return true to continue the request and call
        /// CefRequestCallback::Continue() either in this method or at a later time to
        /// grant or deny the request. Return false to cancel the request immediately.
        /// </summary>
        // protected abstract int OnQuotaRequest(cef_browser_t* browser, cef_string_t* origin_url, long new_size, cef_request_callback_t* callback);
        
        private void on_protocol_execution(cef_request_handler_t* self, cef_browser_t* browser, cef_string_t* url, int* allow_os_execution)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnProtocolExecution
        }
        
        /// <summary>
        /// Called on the UI thread to handle requests for URLs with an unknown
        /// protocol component. Set |allow_os_execution| to true to attempt execution
        /// via the registered OS protocol handler, if any.
        /// SECURITY WARNING: YOU SHOULD USE THIS METHOD TO ENFORCE RESTRICTIONS BASED
        /// ON SCHEME, HOST OR OTHER URL ANALYSIS BEFORE ALLOWING OS EXECUTION.
        /// </summary>
        // protected abstract void OnProtocolExecution(cef_browser_t* browser, cef_string_t* url, int* allow_os_execution);
        
        private int on_certificate_error(cef_request_handler_t* self, cef_browser_t* browser, CefErrorCode cert_error, cef_string_t* request_url, cef_sslinfo_t* ssl_info, cef_request_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnCertificateError
        }
        
        /// <summary>
        /// Called on the UI thread to handle requests for URLs with an invalid
        /// SSL certificate. Return true and call CefRequestCallback::Continue() either
        /// in this method or at a later time to continue or cancel the request. Return
        /// false to cancel the request immediately. If |callback| is empty the error
        /// cannot be recovered from and the request will be canceled automatically.
        /// If CefSettings.ignore_certificate_errors is set all invalid certificates
        /// will be accepted without calling this method.
        /// </summary>
        // protected abstract int OnCertificateError(cef_browser_t* browser, CefErrorCode cert_error, cef_string_t* request_url, cef_sslinfo_t* ssl_info, cef_request_callback_t* callback);
        
        private int on_before_plugin_load(cef_request_handler_t* self, cef_browser_t* browser, cef_string_t* url, cef_string_t* policy_url, cef_web_plugin_info_t* info)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnBeforePluginLoad
        }
        
        /// <summary>
        /// Called on the browser process IO thread before a plugin is loaded. Return
        /// true to block loading of the plugin.
        /// </summary>
        // protected abstract int OnBeforePluginLoad(cef_browser_t* browser, cef_string_t* url, cef_string_t* policy_url, cef_web_plugin_info_t* info);
        
        private void on_plugin_crashed(cef_request_handler_t* self, cef_browser_t* browser, cef_string_t* plugin_path)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnPluginCrashed
        }
        
        /// <summary>
        /// Called on the browser process UI thread when a plugin has crashed.
        /// |plugin_path| is the path of the plugin that crashed.
        /// </summary>
        // protected abstract void OnPluginCrashed(cef_browser_t* browser, cef_string_t* plugin_path);
        
        private void on_render_view_ready(cef_request_handler_t* self, cef_browser_t* browser)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnRenderViewReady
        }
        
        /// <summary>
        /// Called on the browser process UI thread when the render view associated
        /// with |browser| is ready to receive/handle IPC messages in the render
        /// process.
        /// </summary>
        // protected abstract void OnRenderViewReady(cef_browser_t* browser);
        
        private void on_render_process_terminated(cef_request_handler_t* self, cef_browser_t* browser, CefTerminationStatus status)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRequestHandler.OnRenderProcessTerminated
        }
        
        /// <summary>
        /// Called on the browser process UI thread when the render process
        /// terminates unexpectedly. |status| indicates how the process
        /// terminated.
        /// </summary>
        // protected abstract void OnRenderProcessTerminated(cef_browser_t* browser, CefTerminationStatus status);
        
    }
}
