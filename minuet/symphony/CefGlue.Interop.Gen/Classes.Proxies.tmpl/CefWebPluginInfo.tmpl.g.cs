namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Information about a specific web plugin.
    /// </summary>
    public sealed unsafe partial class CefWebPluginInfo
    {
        /// <summary>
        /// Returns the plugin name (i.e. Flash).
        /// </summary>
        public cef_string_userfree* GetName()
        {
            throw new NotImplementedException(); // TODO: CefWebPluginInfo.GetName
        }
        
        /// <summary>
        /// Returns the plugin file path (DLL/bundle/library).
        /// </summary>
        public cef_string_userfree* GetPath()
        {
            throw new NotImplementedException(); // TODO: CefWebPluginInfo.GetPath
        }
        
        /// <summary>
        /// Returns the version of the plugin (may be OS-specific).
        /// </summary>
        public cef_string_userfree* GetVersion()
        {
            throw new NotImplementedException(); // TODO: CefWebPluginInfo.GetVersion
        }
        
        /// <summary>
        /// Returns a description of the plugin from the version information.
        /// </summary>
        public cef_string_userfree* GetDescription()
        {
            throw new NotImplementedException(); // TODO: CefWebPluginInfo.GetDescription
        }
        
    }
}
