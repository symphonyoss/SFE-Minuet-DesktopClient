namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// Implement this interface to provide handler implementations.
    /// </summary>
    public abstract unsafe partial class CefRequestContextHandler
    {
        private cef_cookie_manager_t* get_cookie_manager(cef_request_context_handler_t* self)
        {
            CheckSelf(self);

            var result = GetCookieManager();

            return result != null ? result.ToNative() : null;
        }

        /// <summary>
        /// Called on the IO thread to retrieve the cookie manager. The global cookie
        /// manager will be used if this method returns NULL.
        /// </summary>
        protected virtual CefCookieManager GetCookieManager()
        {
            return null;
        }

        private int on_before_plugin_load(cef_request_context_handler_t* self, cef_string_t* mime_type, cef_string_t* plugin_url, cef_string_t* top_origin_url, cef_web_plugin_info_t* plugin_info, CefPluginPolicy* plugin_policy)
        {
            CheckSelf(self);
            CefPluginPolicy policy = *plugin_policy;
            var retVal = OnBeforePluginLoad(cef_string_t.ToString(mime_type),
                                      cef_string_t.ToString(plugin_url),
                                      cef_string_t.ToString(top_origin_url),
                                      CefWebPluginInfo.FromNative(plugin_info),
                                      ref policy);
            *plugin_policy = policy;
            return retVal ? 1 : 0;
        }

        /// <summary>
        /// Called on multiple browser process threads before a plugin instance is
        /// loaded. |mime_type| is the mime type of the plugin that will be loaded.
        /// |plugin_url| is the content URL that the plugin will load and may be empty.
        /// |top_origin_url| is the URL for the top-level frame that contains the
        /// plugin when loading a specific plugin instance or empty when building the
        /// initial list of enabled plugins for 'navigator.plugins' JavaScript state.
        /// |plugin_info| includes additional information about the plugin that will be
        /// loaded. |plugin_policy| is the recommended policy. Modify |plugin_policy|
        /// and return true to change the policy. Return false to use the recommended
        /// policy. The default plugin policy can be set at runtime using the
        /// `--plugin-policy=[allow|detect|block]` command-line flag. Decisions to mark
        /// a plugin as disabled by setting |plugin_policy| to PLUGIN_POLICY_DISABLED
        /// may be cached when |top_origin_url| is empty. To purge the plugin list
        /// cache and potentially trigger new calls to this method call
        /// CefRequestContext::PurgePluginListCache.
        /// </summary>
        protected virtual bool OnBeforePluginLoad(string mime_type, string plugin_url, string top_origin_url, CefWebPluginInfo plugin_info, ref CefPluginPolicy plugin_policy)
        {
            return false;
        }
    }
}
