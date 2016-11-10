namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class used to implement a custom request handler interface. The methods of
    /// this class will always be called on the IO thread.
    /// </summary>
    public abstract unsafe partial class CefResourceHandler
    {
        private int process_request(cef_resource_handler_t* self, cef_request_t* request, cef_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResourceHandler.ProcessRequest
        }
        
        /// <summary>
        /// Begin processing the request. To handle the request return true and call
        /// CefCallback::Continue() once the response header information is available
        /// (CefCallback::Continue() can also be called from inside this method if
        /// header information is available immediately). To cancel the request return
        /// false.
        /// </summary>
        // protected abstract int ProcessRequest(cef_request_t* request, cef_callback_t* callback);
        
        private void get_response_headers(cef_resource_handler_t* self, cef_response_t* response, long* response_length, cef_string_t* redirectUrl)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResourceHandler.GetResponseHeaders
        }
        
        /// <summary>
        /// Retrieve response header information. If the response length is not known
        /// set |response_length| to -1 and ReadResponse() will be called until it
        /// returns false. If the response length is known set |response_length|
        /// to a positive value and ReadResponse() will be called until it returns
        /// false or the specified number of bytes have been read. Use the |response|
        /// object to set the mime type, http status code and other optional header
        /// values. To redirect the request to a new URL set |redirectUrl| to the new
        /// URL. If an error occured while setting up the request you can call
        /// SetError() on |response| to indicate the error condition.
        /// </summary>
        // protected abstract void GetResponseHeaders(cef_response_t* response, long* response_length, cef_string_t* redirectUrl);
        
        private int read_response(cef_resource_handler_t* self, void* data_out, int bytes_to_read, int* bytes_read, cef_callback_t* callback)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResourceHandler.ReadResponse
        }
        
        /// <summary>
        /// Read response data. If data is available immediately copy up to
        /// |bytes_to_read| bytes into |data_out|, set |bytes_read| to the number of
        /// bytes copied, and return true. To read the data at a later time set
        /// |bytes_read| to 0, return true and call CefCallback::Continue() when the
        /// data is available. To indicate response completion return false.
        /// </summary>
        // protected abstract int ReadResponse(void* data_out, int bytes_to_read, int* bytes_read, cef_callback_t* callback);
        
        private int can_get_cookie(cef_resource_handler_t* self, cef_cookie_t* cookie)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResourceHandler.CanGetCookie
        }
        
        /// <summary>
        /// Return true if the specified cookie can be sent with the request or false
        /// otherwise. If false is returned for any cookie then no cookies will be sent
        /// with the request.
        /// </summary>
        // protected abstract int CanGetCookie(cef_cookie_t* cookie);
        
        private int can_set_cookie(cef_resource_handler_t* self, cef_cookie_t* cookie)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResourceHandler.CanSetCookie
        }
        
        /// <summary>
        /// Return true if the specified cookie returned with the response can be set
        /// or false otherwise.
        /// </summary>
        // protected abstract int CanSetCookie(cef_cookie_t* cookie);
        
        private void cancel(cef_resource_handler_t* self)
        {
            CheckSelf(self);
            throw new NotImplementedException(); // TODO: CefResourceHandler.Cancel
        }
        
        /// <summary>
        /// Request processing has been canceled.
        /// </summary>
        // protected abstract void Cancel();
        
    }
}
