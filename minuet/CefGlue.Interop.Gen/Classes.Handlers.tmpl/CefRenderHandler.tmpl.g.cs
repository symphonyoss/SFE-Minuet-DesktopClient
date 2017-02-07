namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to handle events when window rendering is disabled.
    /// The methods of this class will be called on the UI thread.
    /// </summary>
    public abstract unsafe partial class CefRenderHandler
    {
        private int get_root_screen_rect(cef_render_handler_t* self, cef_browser_t* browser, cef_rect_t* rect)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.GetRootScreenRect
        }
        
        /// <summary>
        /// Called to retrieve the root window rectangle in screen coordinates. Return
        /// true if the rectangle was provided.
        /// </summary>
        // protected abstract int GetRootScreenRect(cef_browser_t* browser, cef_rect_t* rect);
        
        private int get_view_rect(cef_render_handler_t* self, cef_browser_t* browser, cef_rect_t* rect)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.GetViewRect
        }
        
        /// <summary>
        /// Called to retrieve the view rectangle which is relative to screen
        /// coordinates. Return true if the rectangle was provided.
        /// </summary>
        // protected abstract int GetViewRect(cef_browser_t* browser, cef_rect_t* rect);
        
        private int get_screen_point(cef_render_handler_t* self, cef_browser_t* browser, int viewX, int viewY, int* screenX, int* screenY)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.GetScreenPoint
        }
        
        /// <summary>
        /// Called to retrieve the translation from view coordinates to actual screen
        /// coordinates. Return true if the screen coordinates were provided.
        /// </summary>
        // protected abstract int GetScreenPoint(cef_browser_t* browser, int viewX, int viewY, int* screenX, int* screenY);
        
        private int get_screen_info(cef_render_handler_t* self, cef_browser_t* browser, cef_screen_info_t* screen_info)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.GetScreenInfo
        }
        
        /// <summary>
        /// Called to allow the client to fill in the CefScreenInfo object with
        /// appropriate values. Return true if the |screen_info| structure has been
        /// modified.
        /// If the screen info rectangle is left empty the rectangle from GetViewRect
        /// will be used. If the rectangle is still empty or invalid popups may not be
        /// drawn correctly.
        /// </summary>
        // protected abstract int GetScreenInfo(cef_browser_t* browser, cef_screen_info_t* screen_info);
        
        private void on_popup_show(cef_render_handler_t* self, cef_browser_t* browser, int show)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.OnPopupShow
        }
        
        /// <summary>
        /// Called when the browser wants to show or hide the popup widget. The popup
        /// should be shown if |show| is true and hidden if |show| is false.
        /// </summary>
        // protected abstract void OnPopupShow(cef_browser_t* browser, int show);
        
        private void on_popup_size(cef_render_handler_t* self, cef_browser_t* browser, cef_rect_t* rect)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.OnPopupSize
        }
        
        /// <summary>
        /// Called when the browser wants to move or resize the popup widget. |rect|
        /// contains the new location and size in view coordinates.
        /// </summary>
        // protected abstract void OnPopupSize(cef_browser_t* browser, cef_rect_t* rect);
        
        private void on_paint(cef_render_handler_t* self, cef_browser_t* browser, CefPaintElementType type, UIntPtr dirtyRectsCount, cef_rect_t* dirtyRects, void* buffer, int width, int height)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.OnPaint
        }
        
        /// <summary>
        /// Called when an element should be painted. Pixel values passed to this
        /// method are scaled relative to view coordinates based on the value of
        /// CefScreenInfo.device_scale_factor returned from GetScreenInfo. |type|
        /// indicates whether the element is the view or the popup widget. |buffer|
        /// contains the pixel data for the whole image. |dirtyRects| contains the set
        /// of rectangles in pixel coordinates that need to be repainted. |buffer| will
        /// be |width|*|height|*4 bytes in size and represents a BGRA image with an
        /// upper-left origin.
        /// </summary>
        // protected abstract void OnPaint(cef_browser_t* browser, CefPaintElementType type, UIntPtr dirtyRectsCount, cef_rect_t* dirtyRects, void* buffer, int width, int height);
        
        private void on_cursor_change(cef_render_handler_t* self, cef_browser_t* browser, IntPtr cursor, CefCursorType type, cef_cursor_info_t* custom_cursor_info)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.OnCursorChange
        }
        
        /// <summary>
        /// Called when the browser's cursor has changed. If |type| is CT_CUSTOM then
        /// |custom_cursor_info| will be populated with the custom cursor information.
        /// </summary>
        // protected abstract void OnCursorChange(cef_browser_t* browser, IntPtr cursor, CefCursorType type, cef_cursor_info_t* custom_cursor_info);
        
        private int start_dragging(cef_render_handler_t* self, cef_browser_t* browser, cef_drag_data_t* drag_data, CefDragOperationsMask allowed_ops, int x, int y)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.StartDragging
        }
        
        /// <summary>
        /// Called when the user starts dragging content in the web view. Contextual
        /// information about the dragged content is supplied by |drag_data|.
        /// (|x|, |y|) is the drag start location in screen coordinates.
        /// OS APIs that run a system message loop may be used within the
        /// StartDragging call.
        /// Return false to abort the drag operation. Don't call any of
        /// CefBrowserHost::DragSource*Ended* methods after returning false.
        /// Return true to handle the drag operation. Call
        /// CefBrowserHost::DragSourceEndedAt and DragSourceSystemDragEnded either
        /// synchronously or asynchronously to inform the web view that the drag
        /// operation has ended.
        /// </summary>
        // protected abstract int StartDragging(cef_browser_t* browser, cef_drag_data_t* drag_data, CefDragOperationsMask allowed_ops, int x, int y);
        
        private void update_drag_cursor(cef_render_handler_t* self, cef_browser_t* browser, CefDragOperationsMask operation)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.UpdateDragCursor
        }
        
        /// <summary>
        /// Called when the web view wants to update the mouse cursor during a
        /// drag & drop operation. |operation| describes the allowed operation
        /// (none, move, copy, link).
        /// </summary>
        // protected abstract void UpdateDragCursor(cef_browser_t* browser, CefDragOperationsMask operation);
        
        private void on_scroll_offset_changed(cef_render_handler_t* self, cef_browser_t* browser, double x, double y)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.OnScrollOffsetChanged
        }
        
        /// <summary>
        /// Called when the scroll offset has changed.
        /// </summary>
        // protected abstract void OnScrollOffsetChanged(cef_browser_t* browser, double x, double y);
        
        private void on_ime_composition_range_changed(cef_render_handler_t* self, cef_browser_t* browser, cef_range_t* selected_range, UIntPtr character_boundsCount, cef_rect_t* character_bounds)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRenderHandler.OnImeCompositionRangeChanged
        }
        
        /// <summary>
        /// Called when the IME composition range has changed. |selected_range| is the
        /// range of characters that have been selected. |character_bounds| is the
        /// bounds of each character in view coordinates.
        /// </summary>
        // protected abstract void OnImeCompositionRangeChanged(cef_browser_t* browser, cef_range_t* selected_range, UIntPtr character_boundsCount, cef_rect_t* character_bounds);
        
    }
}
