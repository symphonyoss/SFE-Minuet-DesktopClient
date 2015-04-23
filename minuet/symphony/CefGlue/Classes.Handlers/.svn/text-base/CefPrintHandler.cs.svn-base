using System;
using System.Collections.Generic;
using System.Text;

namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summar>
    /// Implement this structure to handle printing on Linux. The functions of this
    /// structure will be called on the browser process UI thread.
    /// </summar>
    public abstract unsafe partial class CefPrintHandler
    {
        private void on_print_settings(cef_print_handler_t* self, cef_print_settings_t* settings, int get_defaults)
        {
            CheckSelf(self);
            CefPrintSettings mSettings = CefPrintSettings.FromNative(settings);

            OnPrintSettings(mSettings, get_defaults != 0);
        }

        ///
        /// Synchronize |settings| with client state. If |get_defaults| is true (1)
        /// then populate |settings| with the default print settings. Do not keep a
        /// reference to |settings| outside of this callback.
        ///
        protected abstract void OnPrintSettings(CefPrintSettings settings, bool getDefaults);

        private int on_print_dialog(cef_print_handler_t* self, int has_selection, cef_print_dialog_callback_t* callback)
        {
            CheckSelf(self);

            CefPrintDialogCallback mCallback = CefPrintDialogCallback.FromNative(callback);

            return OnPrintDialogDelegate(has_selection != 0, mCallback);
        }

        protected abstract int OnPrintDialogDelegate(bool hasSelection, CefPrintDialogCallback callback);

        private int on_print_job(cef_print_handler_t* self, cef_string_t* document_name, cef_string_t* pdf_file_path, cef_print_job_callback_t* callback)
        {
            string mDocumentName = document_name->ToString();
            string mPdfFilePath = document_name->ToString();
            CefPrintJobCallback mCallback = CefPrintJobCallback.FromNative(callback);
            return OnPrintJob(mDocumentName, mPdfFilePath, mCallback);
        }

        ///
        /// Send the print job to the printer. Execute |callback| once the job is
        /// completed. Return true if the job will proceed or false to cancel the job
        // immediately.
        ///
        protected abstract int OnPrintJob(string documentName, string pdfFilePath, CefPrintJobCallback callback);

        private void on_print_reset(cef_print_handler_t* self)
        {
            OnPrintReset();
        }

        ///
        /// Reset client state related to printing.
        ///
        protected abstract void OnPrintReset();
    }
}