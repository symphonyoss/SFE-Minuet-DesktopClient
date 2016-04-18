namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class used for retrieving resources from the resource bundle (*.pak) files
    /// loaded by CEF during startup or via the CefResourceBundleHandler returned
    /// from CefApp::GetResourceBundleHandler. See CefSettings for additional options
    /// related to resource bundle loading. The methods of this class may be called
    /// on any thread unless otherwise indicated.
    /// </summary>
    public sealed unsafe partial class CefResourceBundle
    {
        /// <summary>
        /// Returns the global resource bundle instance.
        /// </summary>
        public static cef_resource_bundle_t* GetGlobal()
        {
            throw new NotImplementedException(); // TODO: CefResourceBundle.GetGlobal
        }
        
        /// <summary>
        /// Returns the localized string for the specified |string_id| or an empty
        /// string if the value is not found. Include cef_pack_strings.h for a listing
        /// of valid string ID values.
        /// </summary>
        public cef_string_userfree* GetLocalizedString(int string_id)
        {
            throw new NotImplementedException(); // TODO: CefResourceBundle.GetLocalizedString
        }
        
        /// <summary>
        /// Retrieves the contents of the specified scale independent |resource_id|.
        /// If the value is found then |data| and |data_size| will be populated and
        /// this method will return true. If the value is not found then this method
        /// will return false. The returned |data| pointer will remain resident in
        /// memory and should not be freed. Include cef_pack_resources.h for a listing
        /// of valid resource ID values.
        /// </summary>
        public int GetDataResource(int resource_id, void** data, UIntPtr* data_size)
        {
            throw new NotImplementedException(); // TODO: CefResourceBundle.GetDataResource
        }
        
        /// <summary>
        /// Retrieves the contents of the specified |resource_id| nearest the scale
        /// factor |scale_factor|. Use a |scale_factor| value of SCALE_FACTOR_NONE for
        /// scale independent resources or call GetDataResource instead. If the value
        /// is found then |data| and |data_size| will be populated and this method will
        /// return true. If the value is not found then this method will return false.
        /// The returned |data| pointer will remain resident in memory and should not
        /// be freed. Include cef_pack_resources.h for a listing of valid resource ID
        /// values.
        /// </summary>
        public int GetDataResourceForScale(int resource_id, cef_scale_factor_t scale_factor, void** data, UIntPtr* data_size)
        {
            throw new NotImplementedException(); // TODO: CefResourceBundle.GetDataResourceForScale
        }
        
    }
}
