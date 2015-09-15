namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to handle events related to browser display state.
    /// The methods of this class will be called on the UI thread.
    /// </summary>
    public abstract unsafe partial class CefDisplayHandler
    {
        private void on_address_change(cef_display_handler_t* self, cef_browser_t* browser, cef_frame_t* frame, cef_string_t* url)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDisplayHandler.OnAddressChange
        }
        
        /// <summary>
        /// Called when a frame's address has changed.
        /// </summary>
        // protected abstract void OnAddressChange(cef_browser_t* browser, cef_frame_t* frame, cef_string_t* url);
        
        private void on_title_change(cef_display_handler_t* self, cef_browser_t* browser, cef_string_t* title)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDisplayHandler.OnTitleChange
        }
        
        /// <summary>
        /// Called when the page title changes.
        /// </summary>
        // protected abstract void OnTitleChange(cef_browser_t* browser, cef_string_t* title);
        
        private void on_favicon_urlchange(cef_display_handler_t* self, cef_browser_t* browser, cef_string_list* icon_urls)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDisplayHandler.OnFaviconURLChange
        }
        
        /// <summary>
        /// Called when the page icon changes.
        /// </summary>
        // protected abstract void OnFaviconURLChange(cef_browser_t* browser, cef_string_list* icon_urls);
        
        private void on_fullscreen_mode_change(cef_display_handler_t* self, cef_browser_t* browser, int fullscreen)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDisplayHandler.OnFullscreenModeChange
        }
        
        /// <summary>
        /// Called when web content in the page has toggled fullscreen mode. If
        /// |fullscreen| is true the content will automatically be sized to fill the
        /// browser content area. If |fullscreen| is false the content will
        /// automatically return to its original size and position. The client is
        /// responsible for resizing the browser if desired.
        /// </summary>
        // protected abstract void OnFullscreenModeChange(cef_browser_t* browser, int fullscreen);
        
        private int on_tooltip(cef_display_handler_t* self, cef_browser_t* browser, cef_string_t* text)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDisplayHandler.OnTooltip
        }
        
        /// <summary>
        /// Called when the browser is about to display a tooltip. |text| contains the
        /// text that will be displayed in the tooltip. To handle the display of the
        /// tooltip yourself return true. Otherwise, you can optionally modify |text|
        /// and then return false to allow the browser to display the tooltip.
        /// When window rendering is disabled the application is responsible for
        /// drawing tooltips and the return value is ignored.
        /// </summary>
        // protected abstract int OnTooltip(cef_browser_t* browser, cef_string_t* text);
        
        private void on_status_message(cef_display_handler_t* self, cef_browser_t* browser, cef_string_t* value)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDisplayHandler.OnStatusMessage
        }
        
        /// <summary>
        /// Called when the browser receives a status message. |value| contains the
        /// text that will be displayed in the status message.
        /// </summary>
        // protected abstract void OnStatusMessage(cef_browser_t* browser, cef_string_t* value);
        
        private int on_console_message(cef_display_handler_t* self, cef_browser_t* browser, cef_string_t* message, cef_string_t* source, int line)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDisplayHandler.OnConsoleMessage
        }
        
        /// <summary>
        /// Called to display a console message. Return true to stop the message from
        /// being output to the console.
        /// </summary>
        // protected abstract int OnConsoleMessage(cef_browser_t* browser, cef_string_t* message, cef_string_t* source, int line);
        
    }
}
