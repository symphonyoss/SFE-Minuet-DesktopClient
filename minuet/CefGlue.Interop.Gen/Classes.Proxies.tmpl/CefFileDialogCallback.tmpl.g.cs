namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Callback interface for asynchronous continuation of file dialog requests.
    /// </summary>
    public sealed unsafe partial class CefFileDialogCallback
    {
        /// <summary>
        /// Continue the file selection. |selected_accept_filter| should be the 0-based
        /// index of the value selected from the accept filters array passed to
        /// CefDialogHandler::OnFileDialog. |file_paths| should be a single value or a
        /// list of values depending on the dialog mode. An empty |file_paths| value is
        /// treated the same as calling Cancel().
        /// </summary>
        public void Continue(int selected_accept_filter, cef_string_list* file_paths)
        {
            throw new NotImplementedException(); // TODO: CefFileDialogCallback.Continue
        }
        
        /// <summary>
        /// Cancel the file selection.
        /// </summary>
        public void Cancel()
        {
            throw new NotImplementedException(); // TODO: CefFileDialogCallback.Cancel
        }
        
    }
}
