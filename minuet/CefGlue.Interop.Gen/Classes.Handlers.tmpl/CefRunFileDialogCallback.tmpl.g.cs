namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Callback interface for CefBrowserHost::RunFileDialog. The methods of this
    /// class will be called on the browser process UI thread.
    /// </summary>
    public abstract unsafe partial class CefRunFileDialogCallback
    {
        private void on_file_dialog_dismissed(cef_run_file_dialog_callback_t* self, int selected_accept_filter, cef_string_list* file_paths)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefRunFileDialogCallback.OnFileDialogDismissed
        }
        
        /// <summary>
        /// Called asynchronously after the file dialog is dismissed.
        /// |selected_accept_filter| is the 0-based index of the value selected from
        /// the accept filters array passed to CefBrowserHost::RunFileDialog.
        /// |file_paths| will be a single value or a list of values depending on the
        /// dialog mode. If the selection was cancelled |file_paths| will be empty.
        /// </summary>
        // protected abstract void OnFileDialogDismissed(int selected_accept_filter, cef_string_list* file_paths);
        
    }
}
