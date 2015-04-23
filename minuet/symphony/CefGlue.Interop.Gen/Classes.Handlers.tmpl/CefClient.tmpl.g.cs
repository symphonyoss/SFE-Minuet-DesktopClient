namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to provide handler implementations.
    /// </summary>
    public abstract unsafe partial class CefClient
    {
        private cef_context_menu_handler_t* get_context_menu_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetContextMenuHandler
        }
        
        /// <summary>
        /// Return the handler for context menus. If no handler is provided the default
        /// implementation will be used.
        /// </summary>
        // protected abstract cef_context_menu_handler_t* GetContextMenuHandler();
        
        private cef_dialog_handler_t* get_dialog_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetDialogHandler
        }
        
        /// <summary>
        /// Return the handler for dialogs. If no handler is provided the default
        /// implementation will be used.
        /// </summary>
        // protected abstract cef_dialog_handler_t* GetDialogHandler();
        
        private cef_display_handler_t* get_display_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetDisplayHandler
        }
        
        /// <summary>
        /// Return the handler for browser display state events.
        /// </summary>
        // protected abstract cef_display_handler_t* GetDisplayHandler();
        
        private cef_download_handler_t* get_download_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetDownloadHandler
        }
        
        /// <summary>
        /// Return the handler for download events. If no handler is returned downloads
        /// will not be allowed.
        /// </summary>
        // protected abstract cef_download_handler_t* GetDownloadHandler();
        
        private cef_drag_handler_t* get_drag_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetDragHandler
        }
        
        /// <summary>
        /// Return the handler for drag events.
        /// </summary>
        // protected abstract cef_drag_handler_t* GetDragHandler();
        
        private cef_focus_handler_t* get_focus_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetFocusHandler
        }
        
        /// <summary>
        /// Return the handler for focus events.
        /// </summary>
        // protected abstract cef_focus_handler_t* GetFocusHandler();
        
        private cef_geolocation_handler_t* get_geolocation_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetGeolocationHandler
        }
        
        /// <summary>
        /// Return the handler for geolocation permissions requests. If no handler is
        /// provided geolocation access will be denied by default.
        /// </summary>
        // protected abstract cef_geolocation_handler_t* GetGeolocationHandler();
        
        private cef_jsdialog_handler_t* get_jsdialog_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetJSDialogHandler
        }
        
        /// <summary>
        /// Return the handler for JavaScript dialogs. If no handler is provided the
        /// default implementation will be used.
        /// </summary>
        // protected abstract cef_jsdialog_handler_t* GetJSDialogHandler();
        
        private cef_keyboard_handler_t* get_keyboard_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetKeyboardHandler
        }
        
        /// <summary>
        /// Return the handler for keyboard events.
        /// </summary>
        // protected abstract cef_keyboard_handler_t* GetKeyboardHandler();
        
        private cef_life_span_handler_t* get_life_span_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetLifeSpanHandler
        }
        
        /// <summary>
        /// Return the handler for browser life span events.
        /// </summary>
        // protected abstract cef_life_span_handler_t* GetLifeSpanHandler();
        
        private cef_load_handler_t* get_load_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetLoadHandler
        }
        
        /// <summary>
        /// Return the handler for browser load status events.
        /// </summary>
        // protected abstract cef_load_handler_t* GetLoadHandler();
        
        private cef_render_handler_t* get_render_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetRenderHandler
        }
        
        /// <summary>
        /// Return the handler for off-screen rendering events.
        /// </summary>
        // protected abstract cef_render_handler_t* GetRenderHandler();
        
        private cef_request_handler_t* get_request_handler(cef_client_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.GetRequestHandler
        }
        
        /// <summary>
        /// Return the handler for browser request events.
        /// </summary>
        // protected abstract cef_request_handler_t* GetRequestHandler();
        
        private int on_process_message_received(cef_client_t* self, cef_browser_t* browser, CefProcessId source_process, cef_process_message_t* message)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefClient.OnProcessMessageReceived
        }
        
        /// <summary>
        /// Called when a new message is received from a different process. Return true
        /// if the message was handled or false otherwise. Do not keep a reference to
        /// or attempt to access the message outside of this callback.
        /// </summary>
        // protected abstract int OnProcessMessageReceived(cef_browser_t* browser, CefProcessId source_process, cef_process_message_t* message);
        
    }
}
