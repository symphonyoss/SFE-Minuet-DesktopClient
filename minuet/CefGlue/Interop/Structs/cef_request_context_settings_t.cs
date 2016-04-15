namespace Xilium.CefGlue.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_request_context_settings_t
    {
        ///
        // Size of this structure.
        ///
        public UIntPtr size;

        ///
        // The location where cache data will be stored on disk. If empty then
        // browsers will be created in "incognito mode" where in-memory caches are
        // used for storage and no data is persisted to disk. HTML5 databases such as
        // localStorage will only persist across sessions if a cache path is
        // specified. To share the global browser cache and related configuration set
        // this value to match the CefSettings.cache_path value.
        ///
        public cef_string_t cache_path;

        ///
        // To persist session cookies (cookies without an expiry date or validity
        // interval) by default when using the global cookie manager set this value to
        // true. Session cookies are generally intended to be transient and most Web
        // browsers do not persist them. Can be set globally using the
        // CefSettings.persist_session_cookies value. This value will be ignored if
        // |cache_path| is empty or if it matches the CefSettings.cache_path value.
        ///
        public int persist_session_cookies;

        ///
        // Set to true (1) to ignore errors related to invalid SSL certificates.
        // Enabling this setting can lead to potential security vulnerabilities like
        // "man in the middle" attacks. Applications that load content from the
        // internet should not enable this setting. Can be set globally using the
        // CefSettings.ignore_certificate_errors value. This value will be ignored if
        // |cache_path| matches the CefSettings.cache_path value.
        ///
        public int ignore_certificate_errors;

        ///
        // Comma delimited ordered list of language codes without any whitespace that
        // will be used in the "Accept-Language" HTTP header. Can be set globally
        // using the CefSettings.accept_language_list value or overridden on a per-
        // browser basis using the CefBrowserSettings.accept_language_list value. If
        // all values are empty then "en-US,en" will be used. This value will be
        // ignored if |cache_path| matches the CefSettings.cache_path value.
        ///
        public cef_string_t accept_language_list;

        ///
        // Specifies the comma separated white list of domains for which the single sign on
        // authentication may be used
        // see https://dev.chromium.org/administrators/policy-list-3#AuthServerWhitelist
        ///
        public cef_string_t auth_server_whitelist;

        ///
        // Kerberos delegation server whitelist
        // see
        // https://dev.chromium.org/administrators/policy-list-3#AuthNegotiateDelegateWhitelist
        ///
        public cef_string_t auth_delegate_whitelist;

        #region Alloc & Free
        private static int _sizeof;

        static cef_request_context_settings_t()
        {
            _sizeof = Marshal.SizeOf(typeof(cef_request_context_settings_t));
        }

        public static cef_request_context_settings_t* Alloc()
        {
            var ptr = (cef_request_context_settings_t*)Marshal.AllocHGlobal(_sizeof);
            *ptr = new cef_request_context_settings_t();
            ptr->size = (UIntPtr)_sizeof;
            return ptr;
        }

        public static void Free(cef_request_context_settings_t* ptr)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }
        #endregion

    }
}

