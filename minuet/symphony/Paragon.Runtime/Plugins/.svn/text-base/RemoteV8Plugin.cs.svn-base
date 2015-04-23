using System;
using Paragon.Runtime.Plugins.TypeConversion;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// A plugin object exposed to V8. This may be a local (loaded into the render process) 
    /// or remote (a proxy for a plugin object loaded into the browser process or a seperate Plugin process) plugin.
    /// </summary>
    public sealed class RemoteV8Plugin : V8Plugin
    {
        private readonly IV8PluginRouter _router;

        public RemoteV8Plugin(IV8PluginRouter router, IPluginContext pluginContext, PluginDescriptor remoteObjectDescriptor)
            : base(pluginContext, remoteObjectDescriptor)
        {
            if (router == null)
            {
                throw new ArgumentNullException("router");
            }

            _router = router;
        }

        protected override void OnAddEventListener(CefV8Context context, string eventName, V8Callback v8Callback)
        {
            _router.AddEventListener(context, Descriptor, eventName, v8Callback);
        }

        protected override void OnRemoveEventListener(CefV8Context context, string eventName, V8Callback v8Callback)
        {
            _router.RemoveEventListener(context, Descriptor, eventName, v8Callback);
        }

        protected override void OnInvokeFunction(CefV8Context context, MethodDescriptor methodDescriptor, CefV8Value[] parameters, V8Callback v8Callback, out CefV8Value returnValue, out string exception)
        {
            returnValue = null;
            exception = null;
            var jsonParameters = CefJsonValueConverter.ToJArray(parameters);
            _router.InvokeFunction(context, Descriptor, methodDescriptor, jsonParameters, v8Callback);
        }
    }
}