namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class used to represent a web response. The methods of this class may be
    /// called on any thread.
    /// </summary>
    public sealed unsafe partial class CefResponse
    {
        /// <summary>
        /// Create a new CefResponse object.
        /// </summary>
        public static cef_response_t* Create()
        {
            throw new NotImplementedException(); // TODO: CefResponse.Create
        }
        
        /// <summary>
        /// Returns true if this object is read-only.
        /// </summary>
        public int IsReadOnly()
        {
            throw new NotImplementedException(); // TODO: CefResponse.IsReadOnly
        }
        
        /// <summary>
        /// Get the response status code.
        /// </summary>
        public int GetStatus()
        {
            throw new NotImplementedException(); // TODO: CefResponse.GetStatus
        }
        
        /// <summary>
        /// Set the response status code.
        /// </summary>
        public void SetStatus(int status)
        {
            throw new NotImplementedException(); // TODO: CefResponse.SetStatus
        }
        
        /// <summary>
        /// Get the response status text.
        /// </summary>
        public cef_string_userfree* GetStatusText()
        {
            throw new NotImplementedException(); // TODO: CefResponse.GetStatusText
        }
        
        /// <summary>
        /// Set the response status text.
        /// </summary>
        public void SetStatusText(cef_string_t* statusText)
        {
            throw new NotImplementedException(); // TODO: CefResponse.SetStatusText
        }
        
        /// <summary>
        /// Get the response mime type.
        /// </summary>
        public cef_string_userfree* GetMimeType()
        {
            throw new NotImplementedException(); // TODO: CefResponse.GetMimeType
        }
        
        /// <summary>
        /// Set the response mime type.
        /// </summary>
        public void SetMimeType(cef_string_t* mimeType)
        {
            throw new NotImplementedException(); // TODO: CefResponse.SetMimeType
        }
        
        /// <summary>
        /// Get the value for the specified response header field.
        /// </summary>
        public cef_string_userfree* GetHeader(cef_string_t* name)
        {
            throw new NotImplementedException(); // TODO: CefResponse.GetHeader
        }
        
        /// <summary>
        /// Get all response header fields.
        /// </summary>
        public void GetHeaderMap(cef_string_multimap* headerMap)
        {
            throw new NotImplementedException(); // TODO: CefResponse.GetHeaderMap
        }
        
        /// <summary>
        /// Set all response header fields.
        /// </summary>
        public void SetHeaderMap(cef_string_multimap* headerMap)
        {
            throw new NotImplementedException(); // TODO: CefResponse.SetHeaderMap
        }
        
    }
}
