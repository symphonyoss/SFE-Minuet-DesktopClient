namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class representing SSL information.
    /// </summary>
    public sealed unsafe partial class CefSslInfo
    {
        /// <summary>
        /// Returns the subject of the X.509 certificate. For HTTPS server
        /// certificates this represents the web server.  The common name of the
        /// subject should match the host name of the web server.
        /// </summary>
        public cef_sslcert_principal_t* GetSubject()
        {
            throw new NotImplementedException(); // TODO: CefSslInfo.GetSubject
        }
        
        /// <summary>
        /// Returns the issuer of the X.509 certificate.
        /// </summary>
        public cef_sslcert_principal_t* GetIssuer()
        {
            throw new NotImplementedException(); // TODO: CefSslInfo.GetIssuer
        }
        
        /// <summary>
        /// Returns the DER encoded serial number for the X.509 certificate. The value
        /// possibly includes a leading 00 byte.
        /// </summary>
        public cef_binary_value_t* GetSerialNumber()
        {
            throw new NotImplementedException(); // TODO: CefSslInfo.GetSerialNumber
        }
        
        /// <summary>
        /// Returns the date before which the X.509 certificate is invalid.
        /// CefTime.GetTimeT() will return 0 if no date was specified.
        /// </summary>
        public cef_time_t GetValidStart()
        {
            throw new NotImplementedException(); // TODO: CefSslInfo.GetValidStart
        }
        
        /// <summary>
        /// Returns the date after which the X.509 certificate is invalid.
        /// CefTime.GetTimeT() will return 0 if no date was specified.
        /// </summary>
        public cef_time_t GetValidExpiry()
        {
            throw new NotImplementedException(); // TODO: CefSslInfo.GetValidExpiry
        }
        
        /// <summary>
        /// Returns the DER encoded data for the X.509 certificate.
        /// </summary>
        public cef_binary_value_t* GetDEREncoded()
        {
            throw new NotImplementedException(); // TODO: CefSslInfo.GetDEREncoded
        }
        
        /// <summary>
        /// Returns the PEM encoded data for the X.509 certificate.
        /// </summary>
        public cef_binary_value_t* GetPEMEncoded()
        {
            throw new NotImplementedException(); // TODO: CefSslInfo.GetPEMEncoded
        }
        
    }
}
