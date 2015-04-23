namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class used to handle file downloads. The methods of this class will called
    /// on the browser process UI thread.
    /// </summary>
    public abstract unsafe partial class CefDownloadHandler
    {
        private void on_before_download(cef_download_handler_t* self, cef_browser_t* browser, cef_download_item_t* download_item, cef_string_t* suggested_name, cef_before_download_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDownloadHandler.OnBeforeDownload
        }
        
        /// <summary>
        /// Called before a download begins. |suggested_name| is the suggested name for
        /// the download file. By default the download will be canceled. Execute
        /// |callback| either asynchronously or in this method to continue the download
        /// if desired. Do not keep a reference to |download_item| outside of this
        /// method.
        /// </summary>
        // protected abstract void OnBeforeDownload(cef_browser_t* browser, cef_download_item_t* download_item, cef_string_t* suggested_name, cef_before_download_callback_t* callback);
        
        private void on_download_updated(cef_download_handler_t* self, cef_browser_t* browser, cef_download_item_t* download_item, cef_download_item_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefDownloadHandler.OnDownloadUpdated
        }
        
        /// <summary>
        /// Called when a download's status or progress information has been updated.
        /// This may be called multiple times before and after OnBeforeDownload().
        /// Execute |callback| either asynchronously or in this method to cancel the
        /// download if desired. Do not keep a reference to |download_item| outside of
        /// this method.
        /// </summary>
        // protected abstract void OnDownloadUpdated(cef_browser_t* browser, cef_download_item_t* download_item, cef_download_item_callback_t* callback);
        
    }
}
