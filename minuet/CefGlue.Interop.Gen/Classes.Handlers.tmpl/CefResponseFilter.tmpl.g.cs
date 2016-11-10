namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Implement this interface to filter resource response content. The methods of
    /// this class will be called on the browser process IO thread.
    /// </summary>
    public abstract unsafe partial class CefResponseFilter
    {
        private int init_filter(cef_response_filter_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResponseFilter.InitFilter
        }
        
        /// <summary>
        /// Initialize the response filter. Will only be called a single time. The
        /// filter will not be installed if this method returns false.
        /// </summary>
        // protected abstract int InitFilter();
        
        private CefResponseFilterStatus filter(cef_response_filter_t* self, void* data_in, UIntPtr data_in_size, UIntPtr* data_in_read, void* data_out, UIntPtr data_out_size, UIntPtr* data_out_written)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResponseFilter.Filter
        }
        
        /// <summary>
        /// Called to filter a chunk of data. |data_in| is the input buffer containing
        /// |data_in_size| bytes of pre-filter data (|data_in| will be NULL if
        /// |data_in_size| is zero). |data_out| is the output buffer that can accept up
        /// to |data_out_size| bytes of filtered output data. Set |data_in_read| to the
        /// number of bytes that were read from |data_in|. Set |data_out_written| to
        /// the number of bytes that were written into |data_out|. If some or all of
        /// the pre-filter data was read successfully but more data is needed in order
        /// to continue filtering (filtered output is pending) return
        /// RESPONSE_FILTER_NEED_MORE_DATA. If some or all of the pre-filter data was
        /// read successfully and all available filtered output has been written return
        /// RESPONSE_FILTER_DONE. If an error occurs during filtering return
        /// RESPONSE_FILTER_ERROR. This method will be called repeatedly until there is
        /// no more data to filter (resource response is complete), |data_in_read|
        /// matches |data_in_size| (all available pre-filter bytes have been read), and
        /// the method returns RESPONSE_FILTER_DONE or RESPONSE_FILTER_ERROR. Do not
        /// keep a reference to the buffers passed to this method.
        /// </summary>
        // protected abstract CefResponseFilterStatus Filter(void* data_in, UIntPtr data_in_size, UIntPtr* data_in_read, void* data_out, UIntPtr data_out_size, UIntPtr* data_out_written);
        
    }
}
