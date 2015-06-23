namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class used to represent the browser process aspects of a browser window. The
    /// methods of this class can only be called in the browser process. They may be
    /// called on any thread in that process unless otherwise indicated in the
    /// comments.
    /// </summary>
    public sealed unsafe partial class CefBrowserHost
    {
        /// <summary>
        /// Create a new browser window using the window parameters specified by
        /// |windowInfo|. All values will be copied internally and the actual window
        /// will be created on the UI thread. If |request_context| is empty the
        /// global request context will be used. This method can be called on any
        /// browser process thread and will not block.
        /// </summary>
        public static int CreateBrowser(cef_window_info_t* windowInfo, cef_client_t* client, cef_string_t* url, cef_browser_settings_t* settings, cef_request_context_t* request_context)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.CreateBrowser
        }
        
        /// <summary>
        /// Create a new browser window using the window parameters specified by
        /// |windowInfo|. If |request_context| is empty the global request context
        /// will be used. This method can only be called on the browser process UI
        /// thread.
        /// </summary>
        public static cef_browser_t* CreateBrowserSync(cef_window_info_t* windowInfo, cef_client_t* client, cef_string_t* url, cef_browser_settings_t* settings, cef_request_context_t* request_context)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.CreateBrowserSync
        }
        
        /// <summary>
        /// Returns the hosted browser object.
        /// </summary>
        public cef_browser_t* GetBrowser()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetBrowser
        }
        
        /// <summary>
        /// Request that the browser close. The JavaScript 'onbeforeunload' event will
        /// be fired. If |force_close| is false the event handler, if any, will be
        /// allowed to prompt the user and the user can optionally cancel the close.
        /// If |force_close| is true the prompt will not be displayed and the close
        /// will proceed. Results in a call to CefLifeSpanHandler::DoClose() if the
        /// event handler allows the close or if |force_close| is true. See
        /// CefLifeSpanHandler::DoClose() documentation for additional usage
        /// information.
        /// </summary>
        public void CloseBrowser(int force_close)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.CloseBrowser
        }
        
        /// <summary>
        /// Set whether the browser is focused.
        /// </summary>
        public void SetFocus(int focus)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SetFocus
        }
        
        /// <summary>
        /// Set whether the window containing the browser is visible
        /// (minimized/unminimized, app hidden/unhidden, etc). Only used on Mac OS X.
        /// </summary>
        public void SetWindowVisibility(int visible)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SetWindowVisibility
        }
        
        /// <summary>
        /// Retrieve the window handle for this browser.
        /// </summary>
        public IntPtr GetWindowHandle()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetWindowHandle
        }
        
        /// <summary>
        /// Retrieve the window handle of the browser that opened this browser. Will
        /// return NULL for non-popup windows. This method can be used in combination
        /// with custom handling of modal windows.
        /// </summary>
        public IntPtr GetOpenerWindowHandle()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetOpenerWindowHandle
        }
        
        /// <summary>
        /// Returns the client for this browser.
        /// </summary>
        public cef_client_t* GetClient()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetClient
        }
        
        /// <summary>
        /// Returns the request context for this browser.
        /// </summary>
        public cef_request_context_t* GetRequestContext()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetRequestContext
        }
        
        /// <summary>
        /// Get the current zoom level. The default zoom level is 0.0. This method can
        /// only be called on the UI thread.
        /// </summary>
        public double GetZoomLevel()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetZoomLevel
        }
        
        /// <summary>
        /// Change the zoom level to the specified value. Specify 0.0 to reset the
        /// zoom level. If called on the UI thread the change will be applied
        /// immediately. Otherwise, the change will be applied asynchronously on the
        /// UI thread.
        /// </summary>
        public void SetZoomLevel(double zoomLevel)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SetZoomLevel
        }
        
        /// <summary>
        /// Call to run a file chooser dialog. Only a single file chooser dialog may be
        /// pending at any given time. |mode| represents the type of dialog to display.
        /// |title| to the title to be used for the dialog and may be empty to show the
        /// default title ("Open" or "Save" depending on the mode). |default_file_path|
        /// is the path with optional directory and/or file name component that will be
        /// initially selected in the dialog. |accept_filters| are used to restrict the
        /// selectable file types and may any combination of (a) valid lower-cased MIME
        /// types (e.g. "text/*" or "image/*"), (b) individual file extensions (e.g.
        /// ".txt" or ".png"), or (c) combined description and file extension delimited
        /// using "|" and ";" (e.g. "Image Types|.png;.gif;.jpg").
        /// |selected_accept_filter| is the 0-based index of the filter that will be
        /// selected by default. |callback| will be executed after the dialog is
        /// dismissed or immediately if another dialog is already pending. The dialog
        /// will be initiated asynchronously on the UI thread.
        /// </summary>
        public void RunFileDialog(CefFileDialogMode mode, cef_string_t* title, cef_string_t* default_file_path, cef_string_list* accept_filters, int selected_accept_filter, cef_run_file_dialog_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.RunFileDialog
        }
        
        /// <summary>
        /// Download the file at |url| using CefDownloadHandler.
        /// </summary>
        public void StartDownload(cef_string_t* url)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.StartDownload
        }
        
        /// <summary>
        /// Print the current browser contents.
        /// </summary>
        public void Print()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.Print
        }
        
        /// <summary>
        /// Search for |searchText|. |identifier| can be used to have multiple searches
        /// running simultaniously. |forward| indicates whether to search forward or
        /// backward within the page. |matchCase| indicates whether the search should
        /// be case-sensitive. |findNext| indicates whether this is the first request
        /// or a follow-up. The CefFindHandler instance, if any, returned via
        /// CefClient::GetFindHandler will be called to report find results.
        /// </summary>
        public void Find(int identifier, cef_string_t* searchText, int forward, int matchCase, int findNext)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.Find
        }
        
        /// <summary>
        /// Cancel all searches that are currently going on.
        /// </summary>
        public void StopFinding(int clearSelection)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.StopFinding
        }
        
        /// <summary>
        /// Open developer tools in its own window. If |inspect_element_at| is non-
        /// empty the element at the specified (x,y) location will be inspected.
        /// </summary>
        public void ShowDevTools(cef_window_info_t* windowInfo, cef_client_t* client, cef_browser_settings_t* settings, cef_point_t* inspect_element_at)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.ShowDevTools
        }
        
        /// <summary>
        /// Explicitly close the developer tools window if one exists for this browser
        /// instance.
        /// </summary>
        public void CloseDevTools()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.CloseDevTools
        }
        
        /// <summary>
        /// Retrieve a snapshot of current navigation entries as values sent to the
        /// specified visitor. If |current_only| is true only the current navigation
        /// entry will be sent, otherwise all navigation entries will be sent.
        /// </summary>
        public void GetNavigationEntries(cef_navigation_entry_visitor_t* visitor, int current_only)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetNavigationEntries
        }
        
        /// <summary>
        /// Set whether mouse cursor change is disabled.
        /// </summary>
        public void SetMouseCursorChangeDisabled(int disabled)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SetMouseCursorChangeDisabled
        }
        
        /// <summary>
        /// Returns true if mouse cursor change is disabled.
        /// </summary>
        public int IsMouseCursorChangeDisabled()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.IsMouseCursorChangeDisabled
        }
        
        /// <summary>
        /// If a misspelled word is currently selected in an editable node calling
        /// this method will replace it with the specified |word|.
        /// </summary>
        public void ReplaceMisspelling(cef_string_t* word)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.ReplaceMisspelling
        }
        
        /// <summary>
        /// Add the specified |word| to the spelling dictionary.
        /// </summary>
        public void AddWordToDictionary(cef_string_t* word)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.AddWordToDictionary
        }
        
        /// <summary>
        /// Returns true if window rendering is disabled.
        /// </summary>
        public int IsWindowRenderingDisabled()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.IsWindowRenderingDisabled
        }
        
        /// <summary>
        /// Notify the browser that the widget has been resized. The browser will first
        /// call CefRenderHandler::GetViewRect to get the new size and then call
        /// CefRenderHandler::OnPaint asynchronously with the updated regions. This
        /// method is only used when window rendering is disabled.
        /// </summary>
        public void WasResized()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.WasResized
        }
        
        /// <summary>
        /// Notify the browser that it has been hidden or shown. Layouting and
        /// CefRenderHandler::OnPaint notification will stop when the browser is
        /// hidden. This method is only used when window rendering is disabled.
        /// </summary>
        public void WasHidden(int hidden)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.WasHidden
        }
        
        /// <summary>
        /// Send a notification to the browser that the screen info has changed. The
        /// browser will then call CefRenderHandler::GetScreenInfo to update the
        /// screen information with the new values. This simulates moving the webview
        /// window from one display to another, or changing the properties of the
        /// current display. This method is only used when window rendering is
        /// disabled.
        /// </summary>
        public void NotifyScreenInfoChanged()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.NotifyScreenInfoChanged
        }
        
        /// <summary>
        /// Invalidate the view. The browser will call CefRenderHandler::OnPaint
        /// asynchronously. This method is only used when window rendering is
        /// disabled.
        /// </summary>
        public void Invalidate(CefPaintElementType type)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.Invalidate
        }
        
        /// <summary>
        /// Send a key event to the browser.
        /// </summary>
        public void SendKeyEvent(cef_key_event_t* @event)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SendKeyEvent
        }
        
        /// <summary>
        /// Send a mouse click event to the browser. The |x| and |y| coordinates are
        /// relative to the upper-left corner of the view.
        /// </summary>
        public void SendMouseClickEvent(cef_mouse_event_t* @event, CefMouseButtonType type, int mouseUp, int clickCount)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SendMouseClickEvent
        }
        
        /// <summary>
        /// Send a mouse move event to the browser. The |x| and |y| coordinates are
        /// relative to the upper-left corner of the view.
        /// </summary>
        public void SendMouseMoveEvent(cef_mouse_event_t* @event, int mouseLeave)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SendMouseMoveEvent
        }
        
        /// <summary>
        /// Send a mouse wheel event to the browser. The |x| and |y| coordinates are
        /// relative to the upper-left corner of the view. The |deltaX| and |deltaY|
        /// values represent the movement delta in the X and Y directions respectively.
        /// In order to scroll inside select popups with window rendering disabled
        /// CefRenderHandler::GetScreenPoint should be implemented properly.
        /// </summary>
        public void SendMouseWheelEvent(cef_mouse_event_t* @event, int deltaX, int deltaY)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SendMouseWheelEvent
        }
        
        /// <summary>
        /// Send a focus event to the browser.
        /// </summary>
        public void SendFocusEvent(int setFocus)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SendFocusEvent
        }
        
        /// <summary>
        /// Send a capture lost event to the browser.
        /// </summary>
        public void SendCaptureLostEvent()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.SendCaptureLostEvent
        }
        
        /// <summary>
        /// Notify the browser that the window hosting it is about to be moved or
        /// resized. This method is only used on Windows and Linux.
        /// </summary>
        public void NotifyMoveOrResizeStarted()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.NotifyMoveOrResizeStarted
        }
        
        /// <summary>
        /// Get the NSTextInputContext implementation for enabling IME on Mac when
        /// window rendering is disabled.
        /// </summary>
        public IntPtr GetNSTextInputContext()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.GetNSTextInputContext
        }
        
        /// <summary>
        /// Handles a keyDown event prior to passing it through the NSTextInputClient
        /// machinery.
        /// </summary>
        public void HandleKeyEventBeforeTextInputClient(IntPtr keyEvent)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.HandleKeyEventBeforeTextInputClient
        }
        
        /// <summary>
        /// Performs any additional actions after NSTextInputClient handles the event.
        /// </summary>
        public void HandleKeyEventAfterTextInputClient(IntPtr keyEvent)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.HandleKeyEventAfterTextInputClient
        }
        
        /// <summary>
        /// Call this method when the user drags the mouse into the web view (before
        /// calling DragTargetDragOver/DragTargetLeave/DragTargetDrop).
        /// |drag_data| should not contain file contents as this type of data is not
        /// allowed to be dragged into the web view. File contents can be removed using
        /// CefDragData::ResetFileContents (for example, if |drag_data| comes from
        /// CefRenderHandler::StartDragging).
        /// This method is only used when window rendering is disabled.
        /// </summary>
        public void DragTargetDragEnter(cef_drag_data_t* drag_data, cef_mouse_event_t* @event, CefDragOperationsMask allowed_ops)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.DragTargetDragEnter
        }
        
        /// <summary>
        /// Call this method each time the mouse is moved across the web view during
        /// a drag operation (after calling DragTargetDragEnter and before calling
        /// DragTargetDragLeave/DragTargetDrop).
        /// This method is only used when window rendering is disabled.
        /// </summary>
        public void DragTargetDragOver(cef_mouse_event_t* @event, CefDragOperationsMask allowed_ops)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.DragTargetDragOver
        }
        
        /// <summary>
        /// Call this method when the user drags the mouse out of the web view (after
        /// calling DragTargetDragEnter).
        /// This method is only used when window rendering is disabled.
        /// </summary>
        public void DragTargetDragLeave()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.DragTargetDragLeave
        }
        
        /// <summary>
        /// Call this method when the user completes the drag operation by dropping
        /// the object onto the web view (after calling DragTargetDragEnter).
        /// The object being dropped is |drag_data|, given as an argument to
        /// the previous DragTargetDragEnter call.
        /// This method is only used when window rendering is disabled.
        /// </summary>
        public void DragTargetDrop(cef_mouse_event_t* @event)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.DragTargetDrop
        }
        
        /// <summary>
        /// Call this method when the drag operation started by a
        /// CefRenderHandler::StartDragging call has ended either in a drop or
        /// by being cancelled. |x| and |y| are mouse coordinates relative to the
        /// upper-left corner of the view. If the web view is both the drag source
        /// and the drag target then all DragTarget* methods should be called before
        /// DragSource* mthods.
        /// This method is only used when window rendering is disabled.
        /// </summary>
        public void DragSourceEndedAt(int x, int y, CefDragOperationsMask op)
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.DragSourceEndedAt
        }
        
        /// <summary>
        /// Call this method when the drag operation started by a
        /// CefRenderHandler::StartDragging call has completed. This method may be
        /// called immediately without first calling DragSourceEndedAt to cancel a
        /// drag operation. If the web view is both the drag source and the drag
        /// target then all DragTarget* methods should be called before DragSource*
        /// mthods.
        /// This method is only used when window rendering is disabled.
        /// </summary>
        public void DragSourceSystemDragEnded()
        {
            throw new NotImplementedException(); // TODO: CefBrowserHost.DragSourceSystemDragEnded
        }
        
    }
}
