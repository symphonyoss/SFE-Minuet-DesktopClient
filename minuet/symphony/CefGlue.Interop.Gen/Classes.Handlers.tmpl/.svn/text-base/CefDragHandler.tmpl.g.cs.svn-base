namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to handle events related to dragging. The methods of
    /// this class will be called on the UI thread.
    /// </summary>
    public abstract unsafe partial class CefDragHandler
    {
        private int on_drag_enter(cef_drag_handler_t* self, cef_browser_t* browser, cef_drag_data_t* dragData, CefDragOperationsMask mask)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDragHandler.OnDragEnter
        }
        
        /// <summary>
        /// Called when an external drag event enters the browser window. |dragData|
        /// contains the drag event data and |mask| represents the type of drag
        /// operation. Return false for default drag handling behavior or true to
        /// cancel the drag event.
        /// </summary>
        // protected abstract int OnDragEnter(cef_browser_t* browser, cef_drag_data_t* dragData, CefDragOperationsMask mask);
        
    }
}
