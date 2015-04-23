namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to handle printing on Linux. The methods of this
    /// class will be called on the browser process UI thread.
    /// </summary>
    public abstract unsafe partial class CefPrintHandler
    {
        private void on_print_settings(cef_print_handler_t* self, cef_print_settings_t* settings, int get_defaults)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefPrintHandler.OnPrintSettings
        }
        
        /// <summary>
        /// Synchronize |settings| with client state. If |get_defaults| is true then
        /// populate |settings| with the default print settings. Do not keep a
        /// reference to |settings| outside of this callback.
        /// </summary>
        // protected abstract void OnPrintSettings(cef_print_settings_t* settings, int get_defaults);
        
        private int on_print_dialog(cef_print_handler_t* self, int has_selection, cef_print_dialog_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefPrintHandler.OnPrintDialog
        }
        
        /// <summary>
        /// Show the print dialog. Execute |callback| once the dialog is dismissed.
        /// Return true if the dialog will be displayed or false to cancel the
        /// printing immediately.
        /// </summary>
        // protected abstract int OnPrintDialog(int has_selection, cef_print_dialog_callback_t* callback);
        
        private int on_print_job(cef_print_handler_t* self, cef_string_t* document_name, cef_string_t* pdf_file_path, cef_print_job_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefPrintHandler.OnPrintJob
        }
        
        /// <summary>
        /// Send the print job to the printer. Execute |callback| once the job is
        /// completed. Return true if the job will proceed or false to cancel the job
        /// immediately.
        /// </summary>
        // protected abstract int OnPrintJob(cef_string_t* document_name, cef_string_t* pdf_file_path, cef_print_job_callback_t* callback);
        
        private void on_print_reset(cef_print_handler_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefPrintHandler.OnPrintReset
        }
        
        /// <summary>
        /// Reset client state related to printing.
        /// </summary>
        // protected abstract void OnPrintReset();
        
    }
}
