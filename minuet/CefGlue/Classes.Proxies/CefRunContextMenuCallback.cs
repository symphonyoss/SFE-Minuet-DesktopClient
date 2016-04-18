namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Callback interface used for continuation of custom context menu display.
    /// </summary>
    public sealed unsafe partial class CefRunContextMenuCallback
    {
        /// <summary>
        /// Complete context menu display by selecting the specified |command_id| and
        /// |event_flags|.
        /// </summary>
        public void Continue(int command_id, CefEventFlags event_flags)
        {
            cef_run_context_menu_callback_t.cont(_self, command_id, event_flags);
        }
        
        /// <summary>
        /// Cancel context menu display.
        /// </summary>
        public void Cancel()
        {
            cef_run_context_menu_callback_t.cancel(_self);
        }
    }
}
