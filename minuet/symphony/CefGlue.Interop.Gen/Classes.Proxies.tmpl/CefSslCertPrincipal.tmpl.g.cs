namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class representing the issuer or subject field of an X.509 certificate.
    /// </summary>
    public sealed unsafe partial class CefSslCertPrincipal
    {
        /// <summary>
        /// Returns a name that can be used to represent the issuer.  It tries in this
        /// order: CN, O and OU and returns the first non-empty one found.
        /// </summary>
        public cef_string_userfree* GetDisplayName()
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetDisplayName
        }
        
        /// <summary>
        /// Returns the common name.
        /// </summary>
        public cef_string_userfree* GetCommonName()
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetCommonName
        }
        
        /// <summary>
        /// Returns the locality name.
        /// </summary>
        public cef_string_userfree* GetLocalityName()
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetLocalityName
        }
        
        /// <summary>
        /// Returns the state or province name.
        /// </summary>
        public cef_string_userfree* GetStateOrProvinceName()
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetStateOrProvinceName
        }
        
        /// <summary>
        /// Returns the country name.
        /// </summary>
        public cef_string_userfree* GetCountryName()
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetCountryName
        }
        
        /// <summary>
        /// Retrieve the list of street addresses.
        /// </summary>
        public void GetStreetAddresses(cef_string_list* addresses)
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetStreetAddresses
        }
        
        /// <summary>
        /// Retrieve the list of organization names.
        /// </summary>
        public void GetOrganizationNames(cef_string_list* names)
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetOrganizationNames
        }
        
        /// <summary>
        /// Retrieve the list of organization unit names.
        /// </summary>
        public void GetOrganizationUnitNames(cef_string_list* names)
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetOrganizationUnitNames
        }
        
        /// <summary>
        /// Retrieve the list of domain components.
        /// </summary>
        public void GetDomainComponents(cef_string_list* components)
        {
            throw new NotImplementedException(); // TODO: CefSslCertPrincipal.GetDomainComponents
        }
        
    }
}
