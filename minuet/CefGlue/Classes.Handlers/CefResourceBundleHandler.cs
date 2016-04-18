namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// Class used to implement a custom resource bundle interface. The methods of
    /// this class may be called on multiple threads.
    /// </summary>
    public abstract unsafe partial class CefResourceBundleHandler
    {
        private int get_localized_string(cef_resource_bundle_handler_t* self, int message_id, cef_string_t* @string)
        {
            CheckSelf(self);

            var value = GetLocalizedString(message_id);

            if (value != null)
            {
                cef_string_t.Copy(value, @string);
                return 1;
            }
            else return 0;
        }

        /// <summary>
        /// Called to retrieve a localized translation for the string specified by
        /// |message_id|. To provide the translation set |string| to the translation
        /// string and return true. To use the default translation return false.
        /// Supported message IDs are listed in cef_pack_strings.h.
        /// </summary>
        protected virtual string GetLocalizedString(int messageId)
        {
            return null;
        }


        private int get_data_resource(cef_resource_bundle_handler_t* self, int resource_id, void** data, UIntPtr* data_size)
        {
            CheckSelf(self);
            int size = 0;
            byte[] buffer = null;
            var retVal = GetDataResource( resource_id, out buffer, out size);
            if (retVal && buffer != null && size > 0)
            {
               *data_size = (UIntPtr)(size);
               *data = (void*)Marshal.AllocHGlobal(size);
               Marshal.Copy(buffer, 0, (IntPtr)(*data), size);
            }
            return retVal ? 1 : 0;
        }

        /// <summary>
        /// Called to retrieve data for the resource specified by |resource_id|. To
        /// provide the resource data set |data| and |data_size| to the data pointer
        /// and size respectively and return true. To use the default resource data
        /// return false. The resource data will not be copied and must remain resident
        /// in memory. Supported resource IDs are listed in cef_pack_resources.h.
        /// </summary>
        protected virtual bool GetDataResource(int resource_id, out byte[] data, out int data_size)
        {
            data = null;
            data_size = 0;
            return false;
        }

        private int get_data_resource_for_scale(cef_resource_bundle_handler_t* self, int resource_id, CefScaleFactor scale_factor, void** data, UIntPtr* data_size)
        {
            CheckSelf(self);
            int size = 0;
            byte[] buffer = null;
            var retVal = GetDataResourceForScale(resource_id, scale_factor, out buffer, out size);
            if (retVal && buffer != null && size > 0)
            {
                *data_size = (UIntPtr)(size);
                *data = (void*)Marshal.AllocHGlobal(size);
                Marshal.Copy(buffer, 0, (IntPtr)(*data), size);
            }
            return retVal ? 1 : 0;
        }

        /// <summary>
        /// Called to retrieve data for the specified |resource_id| nearest the scale
        /// factor |scale_factor|. To provide the resource data set |data| and
        /// |data_size| to the data pointer and size respectively and return true. To
        /// use the default resource data return false. The resource data will not be
        /// copied and must remain resident in memory. Include cef_pack_resources.h for
        /// a listing of valid resource ID values.
        /// </summary>
        protected virtual bool GetDataResourceForScale(int resource_id, CefScaleFactor scale_factor, out byte[] data, out int data_size)
        {
            data = null;
            data_size = 0;
            return false;
        }
    }
}
