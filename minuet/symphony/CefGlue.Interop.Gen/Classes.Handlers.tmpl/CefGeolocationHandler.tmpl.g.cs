namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to handle events related to geolocation permission
    /// requests. The methods of this class will be called on the browser process UI
    /// thread.
    /// </summary>
    public abstract unsafe partial class CefGeolocationHandler
    {
        private int on_request_geolocation_permission(cef_geolocation_handler_t* self, cef_browser_t* browser, cef_string_t* requesting_url, int request_id, cef_geolocation_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefGeolocationHandler.OnRequestGeolocationPermission
        }
        
        /// <summary>
        /// Called when a page requests permission to access geolocation information.
        /// |requesting_url| is the URL requesting permission and |request_id| is the
        /// unique ID for the permission request. Return true and call
        /// CefGeolocationCallback::Continue() either in this method or at a later
        /// time to continue or cancel the request. Return false to cancel the request
        /// immediately.
        /// </summary>
        // protected abstract int OnRequestGeolocationPermission(cef_browser_t* browser, cef_string_t* requesting_url, int request_id, cef_geolocation_callback_t* callback);
        
        private void on_cancel_geolocation_permission(cef_geolocation_handler_t* self, cef_browser_t* browser, cef_string_t* requesting_url, int request_id)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefGeolocationHandler.OnCancelGeolocationPermission
        }
        
        /// <summary>
        /// Called when a geolocation access request is canceled. |requesting_url| is
        /// the URL that originally requested permission and |request_id| is the unique
        /// ID for the permission request.
        /// </summary>
        // protected abstract void OnCancelGeolocationPermission(cef_browser_t* browser, cef_string_t* requesting_url, int request_id);
        
    }
}
