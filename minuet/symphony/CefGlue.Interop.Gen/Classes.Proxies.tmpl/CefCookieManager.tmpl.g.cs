namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// Class used for managing cookies. The methods of this class may be called on
    /// any thread unless otherwise indicated.
    /// </summary>
    public sealed unsafe partial class CefCookieManager
    {
        /// <summary>
        /// Returns the global cookie manager. By default data will be stored at
        /// CefSettings.cache_path if specified or in memory otherwise. If |callback|
        /// is non-NULL it will be executed asnychronously on the IO thread after the
        /// manager's storage has been initialized. Using this method is equivalent to
        /// calling CefRequestContext::GetGlobalContext()->GetDefaultCookieManager().
        /// </summary>
        public static cef_cookie_manager_t* GetGlobalManager(cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.GetGlobalManager
        }
        
        /// <summary>
        /// Creates a new cookie manager. If |path| is empty data will be stored in
        /// memory only. Otherwise, data will be stored at the specified |path|. To
        /// persist session cookies (cookies without an expiry date or validity
        /// interval) set |persist_session_cookies| to true. Session cookies are
        /// generally intended to be transient and most Web browsers do not persist
        /// them. If |callback| is non-NULL it will be executed asnychronously on the
        /// IO thread after the manager's storage has been initialized.
        /// </summary>
        public static cef_cookie_manager_t* CreateManager(cef_string_t* path, int persist_session_cookies, cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.CreateManager
        }
        
        /// <summary>
        /// Set the schemes supported by this manager. The default schemes ("http",
        /// "https", "ws" and "wss") will always be supported. If |callback| is non-
        /// NULL it will be executed asnychronously on the IO thread after the change
        /// has been applied. Must be called before any cookies are accessed.
        /// </summary>
        public void SetSupportedSchemes(cef_string_list* schemes, cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.SetSupportedSchemes
        }
        
        /// <summary>
        /// Visit all cookies on the IO thread. The returned cookies are ordered by
        /// longest path, then by earliest creation date. Returns false if cookies
        /// cannot be accessed.
        /// </summary>
        public int VisitAllCookies(cef_cookie_visitor_t* visitor)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.VisitAllCookies
        }
        
        /// <summary>
        /// Visit a subset of cookies on the IO thread. The results are filtered by the
        /// given url scheme, host, domain and path. If |includeHttpOnly| is true
        /// HTTP-only cookies will also be included in the results. The returned
        /// cookies are ordered by longest path, then by earliest creation date.
        /// Returns false if cookies cannot be accessed.
        /// </summary>
        public int VisitUrlCookies(cef_string_t* url, int includeHttpOnly, cef_cookie_visitor_t* visitor)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.VisitUrlCookies
        }
        
        /// <summary>
        /// Sets a cookie given a valid URL and explicit user-provided cookie
        /// attributes. This function expects each attribute to be well-formed. It will
        /// check for disallowed characters (e.g. the ';' character is disallowed
        /// within the cookie value attribute) and fail without setting the cookie if
        /// such characters are found. If |callback| is non-NULL it will be executed
        /// asnychronously on the IO thread after the cookie has been set. Returns
        /// false if an invalid URL is specified or if cookies cannot be accessed.
        /// </summary>
        public int SetCookie(cef_string_t* url, cef_cookie_t* cookie, cef_set_cookie_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.SetCookie
        }
        
        /// <summary>
        /// Delete all cookies that match the specified parameters. If both |url| and
        /// |cookie_name| values are specified all host and domain cookies matching
        /// both will be deleted. If only |url| is specified all host cookies (but not
        /// domain cookies) irrespective of path will be deleted. If |url| is empty all
        /// cookies for all hosts and domains will be deleted. If |callback| is
        /// non-NULL it will be executed asnychronously on the IO thread after the
        /// cookies have been deleted. Returns false if a non-empty invalid URL is
        /// specified or if cookies cannot be accessed. Cookies can alternately be
        /// deleted using the Visit*Cookies() methods.
        /// </summary>
        public int DeleteCookies(cef_string_t* url, cef_string_t* cookie_name, cef_delete_cookies_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.DeleteCookies
        }
        
        /// <summary>
        /// Sets the directory path that will be used for storing cookie data. If
        /// |path| is empty data will be stored in memory only. Otherwise, data will be
        /// stored at the specified |path|. To persist session cookies (cookies without
        /// an expiry date or validity interval) set |persist_session_cookies| to true.
        /// Session cookies are generally intended to be transient and most Web
        /// browsers do not persist them. If |callback| is non-NULL it will be executed
        /// asnychronously on the IO thread after the manager's storage has been
        /// initialized. Returns false if cookies cannot be accessed.
        /// </summary>
        public int SetStoragePath(cef_string_t* path, int persist_session_cookies, cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.SetStoragePath
        }
        
        /// <summary>
        /// Flush the backing store (if any) to disk. If |callback| is non-NULL it will
        /// be executed asnychronously on the IO thread after the flush is complete.
        /// Returns false if cookies cannot be accessed.
        /// </summary>
        public int FlushStore(cef_completion_callback_t* callback)
        {
            throw new NotImplementedException(); // TODO: CefCookieManager.FlushStore
        }
        
    }
}
