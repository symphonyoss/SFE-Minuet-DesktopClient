namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Callback interface for CefBrowserHost::PrintToPDF. The methods of this class
    /// will be called on the browser process UI thread.
    /// </summary>
    public abstract unsafe partial class CefPdfPrintCallback
    {
        private void on_pdf_print_finished(cef_pdf_print_callback_t* self, cef_string_t* path, int ok)
        {
            CheckSelf(self);

            OnPdfPrintFinished(cef_string_t.ToString(path), ok != 0);
        }

        // OnPdfPrintFinished
        protected abstract void OnPdfPrintFinished(string path, bool ok);        
    }
}
