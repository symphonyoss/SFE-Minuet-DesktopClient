namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class used to make a URL request. URL requests are not associated with a
    /// browser instance so no CefClient callbacks will be executed. URL requests
    /// can be created on any valid CEF thread in either the browser or render
    /// process. Once created the methods of the URL request object must be accessed
    /// on the same thread that created it.
    /// </summary>
    public sealed unsafe partial class CefUrlRequest
    {
        /// <summary>
        /// Create a new URL request. Only GET, POST, HEAD, DELETE and PUT request
        /// methods are supported. Multiple post data elements are not supported and
        /// elements of type PDE_TYPE_FILE are only supported for requests originating
        /// from the browser process. Requests originating from the render process will
        /// receive the same handling as requests originating from Web content -- if
        /// the response contains Content-Disposition or Mime-Type header values that
        /// would not normally be rendered then the response may receive special
        /// handling inside the browser (for example, via the file download code path
        /// instead of the URL request code path). The |request| object will be marked
        /// as read-only after calling this method. In the browser process if
        /// |request_context| is empty the global request context will be used. In the
        /// render process |request_context| must be empty and the context associated
        /// with the current renderer process' browser will be used.
        /// </summary>
        public static cef_urlrequest_t* Create(cef_request_t* request, cef_urlrequest_client_t* client, cef_request_context_t* request_context)
        {
            throw new NotImplementedException(); // TODO: CefUrlRequest.Create
        }
        
        /// <summary>
        /// Returns the request object used to create this URL request. The returned
        /// object is read-only and should not be modified.
        /// </summary>
        public cef_request_t* GetRequest()
        {
            throw new NotImplementedException(); // TODO: CefUrlRequest.GetRequest
        }
        
        /// <summary>
        /// Returns the client.
        /// </summary>
        public cef_urlrequest_client_t* GetClient()
        {
            throw new NotImplementedException(); // TODO: CefUrlRequest.GetClient
        }
        
        /// <summary>
        /// Returns the request status.
        /// </summary>
        public CefUrlRequestStatus GetRequestStatus()
        {
            throw new NotImplementedException(); // TODO: CefUrlRequest.GetRequestStatus
        }
        
        /// <summary>
        /// Returns the request error if status is UR_CANCELED or UR_FAILED, or 0
        /// otherwise.
        /// </summary>
        public CefErrorCode GetRequestError()
        {
            throw new NotImplementedException(); // TODO: CefUrlRequest.GetRequestError
        }
        
        /// <summary>
        /// Returns the response, or NULL if no response information is available.
        /// Response information will only be available after the upload has completed.
        /// The returned object is read-only and should not be modified.
        /// </summary>
        public cef_response_t* GetResponse()
        {
            throw new NotImplementedException(); // TODO: CefUrlRequest.GetResponse
        }
        
        /// <summary>
        /// Cancel the request.
        /// </summary>
        public void Cancel()
        {
            throw new NotImplementedException(); // TODO: CefUrlRequest.Cancel
        }
        
    }
}
