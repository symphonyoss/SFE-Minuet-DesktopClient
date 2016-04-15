namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to handle events related to JavaScript dialogs. The
    /// methods of this class will be called on the UI thread.
    /// </summary>
    public abstract unsafe partial class CefJSDialogHandler
    {
        private int on_jsdialog(cef_jsdialog_handler_t* self, cef_browser_t* browser, cef_string_t* origin_url, cef_string_t* accept_lang, CefJSDialogType dialog_type, cef_string_t* message_text, cef_string_t* default_prompt_text, cef_jsdialog_callback_t* callback, int* suppress_message)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefJSDialogHandler.OnJSDialog
        }
        
        /// <summary>
        /// Called to run a JavaScript dialog. The |default_prompt_text| value will be
        /// specified for prompt dialogs only. Set |suppress_message| to true and
        /// return false to suppress the message (suppressing messages is preferable
        /// to immediately executing the callback as this is used to detect presumably
        /// malicious behavior like spamming alert messages in onbeforeunload). Set
        /// |suppress_message| to false and return false to use the default
        /// implementation (the default implementation will show one modal dialog at a
        /// time and suppress any additional dialog requests until the displayed dialog
        /// is dismissed). Return true if the application will use a custom dialog or
        /// if the callback has been executed immediately. Custom dialogs may be either
        /// modal or modeless. If a custom dialog is used the application must execute
        /// |callback| once the custom dialog is dismissed.
        /// </summary>
        // protected abstract int OnJSDialog(cef_browser_t* browser, cef_string_t* origin_url, cef_string_t* accept_lang, CefJSDialogType dialog_type, cef_string_t* message_text, cef_string_t* default_prompt_text, cef_jsdialog_callback_t* callback, int* suppress_message);
        
        private int on_before_unload_dialog(cef_jsdialog_handler_t* self, cef_browser_t* browser, cef_string_t* message_text, int is_reload, cef_jsdialog_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefJSDialogHandler.OnBeforeUnloadDialog
        }
        
        /// <summary>
        /// Called to run a dialog asking the user if they want to leave a page. Return
        /// false to use the default dialog implementation. Return true if the
        /// application will use a custom dialog or if the callback has been executed
        /// immediately. Custom dialogs may be either modal or modeless. If a custom
        /// dialog is used the application must execute |callback| once the custom
        /// dialog is dismissed.
        /// </summary>
        // protected abstract int OnBeforeUnloadDialog(cef_browser_t* browser, cef_string_t* message_text, int is_reload, cef_jsdialog_callback_t* callback);
        
        private void on_reset_dialog_state(cef_jsdialog_handler_t* self, cef_browser_t* browser)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefJSDialogHandler.OnResetDialogState
        }
        
        /// <summary>
        /// Called to cancel any pending dialogs and reset any saved dialog state. Will
        /// be called due to events like page navigation irregardless of whether any
        /// dialogs are currently pending.
        /// </summary>
        // protected abstract void OnResetDialogState(cef_browser_t* browser);
        
        private void on_dialog_closed(cef_jsdialog_handler_t* self, cef_browser_t* browser)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefJSDialogHandler.OnDialogClosed
        }
        
        /// <summary>
        /// Called when the default implementation dialog is closed.
        /// </summary>
        // protected abstract void OnDialogClosed(cef_browser_t* browser);
        
    }
}
