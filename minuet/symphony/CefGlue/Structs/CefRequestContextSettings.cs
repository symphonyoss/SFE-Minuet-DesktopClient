namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xilium.CefGlue.Interop;

    public sealed unsafe class CefRequestContextSettings
    {
        private cef_request_context_settings_t* _self;
        public CefRequestContextSettings()
        {
            _self = cef_request_context_settings_t.Alloc();
        }

        internal CefRequestContextSettings(cef_request_context_settings_t* ptr)
        {
            _self = ptr;
        }

        public string CachePath
        {
            get
            {
                return cef_string_t.ToString(&_self->cache_path); 
            }
            set
            {
                cef_string_t.Copy(value, &_self->cache_path);
            }
        }

        public bool PersistSessionCookies
        {
            get
            {
                return _self->persist_session_cookies != 0;
            }
            set
            {
                _self->persist_session_cookies = value ? 1 : 0;
            }
        }

        public bool IgnoreCertificateErrors
        {
            get
            {
                return _self->ignore_certificate_errors != 0;
            }
            set
            {
                _self->ignore_certificate_errors = value ? 1 : 0;
            }
        }


        public string AcceptLanguageList
        {
            get
            {
                return cef_string_t.ToString(&_self->accept_language_list);
            }
            set
            {
                cef_string_t.Copy(value, &_self->accept_language_list);
            }
        }

        internal void Dispose()
        {
            _self = null;
        }

        internal cef_request_context_settings_t* ToNative()
        {
            return _self;
        }

    }
}
