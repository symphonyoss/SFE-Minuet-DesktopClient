namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// A request context provides request handling for a set of related browser
    /// objects. A request context is specified when creating a new browser object
    /// via the CefBrowserHost static factory methods. Browser objects with different
    /// request contexts will never be hosted in the same render process. Browser
    /// objects with the same request context may or may not be hosted in the same
    /// render process depending on the process model. Browser objects created
    /// indirectly via the JavaScript window.open function or targeted links will
    /// share the same render process and the same request context as the source
    /// browser. When running in single-process mode there is only a single render
    /// process (the main process) and so all browsers created in single-process mode
    /// will share the same request context. This will be the first request context
    /// passed into a CefBrowserHost static factory method and all other request
    /// context objects will be ignored.
    /// </summary>
    public sealed unsafe partial class CefRequestContext
    {
        /// <summary>
        /// Returns the global context object.
        /// </summary>
        public static CefRequestContext GetGlobalContext()
        {
            return CefRequestContext.FromNative(
                cef_request_context_t.get_global_context()
                );
        }

        /// <summary>
        /// Creates a new context object with the specified handler.
        /// </summary>
        public static CefRequestContext CreateContext(CefRequestContextSettings settings, CefRequestContextHandler handler)
        {
            return CefRequestContext.FromNative(
                cef_request_context_t.create_context(
                settings != null ? settings.ToNative() : null,
                    handler != null ? handler.ToNative() : null
                    )
                );
        }

        /// <summary>
        /// Returns true if this object is pointing to the same context as |that|
        /// object.
        /// </summary>
        public bool IsSame(CefRequestContext other)
        {
            if (other == null) return false;

            return cef_request_context_t.is_same(_self, other.ToNative()) != 0;
        }

        /// <summary>
        /// Returns true if this object is the global context.
        /// </summary>
        public bool IsGlobal
        {
            get
            {
                return cef_request_context_t.is_global(_self) != 0;
            }
        }

        /// <summary>
        /// Returns the handler for this context if any.
        /// </summary>
        public CefRequestContextHandler GetHandler()
        {
            return CefRequestContextHandler.FromNativeOrNull(
                cef_request_context_t.get_handler(_self)
                );
        }

        /// <summary>
        /// Returns true if a preference with the specified |name| exists. This method
        /// must be called on the browser process UI thread.
        /// </summary>
        public bool HasPreference(string name)
        {
            fixed (char* name_str = name)
            {
                var n_name = new cef_string_t(name_str, name.Length);
                return cef_request_context_t.has_preference(_self, &n_name) != 0;
            }
        }

        /// <summary>
        /// Returns the value for the preference with the specified |name|. Returns
        /// NULL if the preference does not exist. The returned object contains a copy
        /// of the underlying preference value and modifications to the returned object
        /// will not modify the underlying preference value. This method must be called
        /// on the browser process UI thread.
        /// </summary>
        public CefValue GetPreference(string name)
        {
            fixed (char* name_str = name)
            {
                var n_name = new cef_string_t(name_str, name.Length);
                var n_value = cef_request_context_t.get_preference(_self, &n_name);
                return CefValue.FromNative(n_value);
            }
        }

        /// <summary>
        /// Returns all preferences as a dictionary. If |include_defaults| is true then
        /// preferences currently at their default value will be included. The returned
        /// object contains a copy of the underlying preference values and
        /// modifications to the returned object will not modify the underlying
        /// preference values. This method must be called on the browser process UI
        /// thread.
        /// </summary>
        public CefDictionaryValue GetAllPreferences(int include_defaults)
        {
            var n_value = cef_request_context_t.get_all_preferences(_self, include_defaults);
            return CefDictionaryValue.FromNative(n_value);
        }

        /// <summary>
        /// Returns true if the preference with the specified |name| can be modified
        /// using SetPreference. As one example preferences set via the command-line
        /// usually cannot be modified. This method must be called on the browser
        /// process UI thread.
        /// </summary>
        public bool CanSetPreference(string name)
        {
            fixed (char* name_str = name)
            {
                var n_name = new cef_string_t(name_str, name.Length);
                return cef_request_context_t.can_set_preference(_self, &n_name) != 0;
            }
        }

        /// <summary>
        /// Set the |value| associated with preference |name|. Returns true if the
        /// value is set successfully and false otherwise. If |value| is NULL the
        /// preference will be restored to its default value. If setting the preference
        /// fails then |error| will be populated with a detailed description of the
        /// problem. This method must be called on the browser process UI thread.
        /// </summary>
        public bool SetPreference(string name, CefValue value, string error)
        {
            fixed (char* name_str = name)
            {
                fixed (char* error_str = error)
                {
                    var n_name = new cef_string_t(name_str, name.Length);
                    var n_error = new cef_string_t(error_str, error.Length);
                    return cef_request_context_t.set_preference(_self, &n_name, value.ToNative(), &n_error) != 0;
                }
            }
        }
    }
}
