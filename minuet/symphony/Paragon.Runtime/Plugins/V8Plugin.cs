using System;
using System.Collections.Concurrent;
using System.Linq;
using Paragon.Plugins;
using Paragon.Runtime.Plugins.TypeConversion;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public abstract class V8Plugin : IV8Plugin
    {
        protected static readonly ILogger Logger = ParagonLogManager.GetLogger();

        /// <summary>
        /// Properties added by JS that is consuming this object
        /// </summary>
        private readonly ConcurrentDictionary<string, CefV8Value> _expandoProperties =
            new ConcurrentDictionary<string, CefV8Value>();

        private readonly V8RetainedCallbackCollection _retainedCallbacks;

        private bool _disposed;

        protected V8Plugin(IPluginContext pluginContext, PluginDescriptor descriptor)
        {
            if (pluginContext == null)
            {
                throw new ArgumentNullException("pluginContext");
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            PluginContext = pluginContext;
            Descriptor = descriptor;

            _retainedCallbacks = new V8RetainedCallbackCollection();
        }

        /// <summary>
        /// Returns a descriptor for the plugin.
        /// </summary>
        public PluginDescriptor Descriptor { get; private set; }

        public IPluginContext PluginContext { get; private set; }

        /// <summary>
        /// Add a JavaScript listener function to a native event.
        /// </summary>
        /// <param name="context">The current V8 JavaScript context.</param>
        /// <param name="eventName">The name of the event to attach the function to.</param>
        /// <param name="callback"> The function to attach to the event.</param>
        public void AddEventListener(CefV8Context context, string eventName, CefV8Value callback)
        {
            if (string.IsNullOrEmpty(eventName) || !Descriptor.Events.Contains(eventName))
            {
                throw new Exception("Unknown event name");
            }
            if (callback == null)
            {
                throw new Exception("Missing event listener");
            }

            var v8Callback = _retainedCallbacks.GetOrCreateCallback(this, eventName, callback, V8CallbackType.EventListener);

            // Execute the call through the router, so that the router keeps track of the call
            OnAddEventListener(context, eventName, v8Callback);
        }

        /// <summary>
        /// Remove a JavaScript listener function from a native event.
        /// </summary>
        /// <param name="context">The current V8 JavaScript context.</param>
        /// <param name="eventName">The name of the event to detach the function from.</param>
        /// <param name="callback"> The function to detach from the event.</param>
        public void RemoveEventListener(CefV8Context context, string eventName, CefV8Value callback)
        {
            if (string.IsNullOrEmpty(eventName) || !Descriptor.Events.Contains(eventName))
            {
                throw new Exception("Unknown event name");
            }

            if (callback == null)
            {
                throw new Exception("Missing event listener");
            }

            var v8Callback = _retainedCallbacks.GetAndRemoveCallback(eventName, callback);

            // Execute the call through the router, so that the router keeps track of the call
            OnRemoveEventListener(context, eventName, v8Callback);
        }

        /// <summary>
        /// Determines if the plugin already has a particular JavaScript function added as a listener.
        /// </summary>
        /// <param name="eventName">The name of the event to check.</param>
        /// <param name="callback">The function to check has already been added.</param>
        /// <returns>True if the function was already added; false otherwise.</returns>
        public bool HasEventListener(string eventName, CefV8Value callback)
        {
            return _retainedCallbacks.HasCallback(eventName, callback);
        }

        /// <summary>
        /// Determines if the plugin has any JavaScript listener functions added for a particular event.
        /// </summary>
        /// <param name="eventName">The name of the event to check.</param>
        /// <returns>True if the event has any JavaScript listener functions; false otherwise.</returns>
        public bool HasEventListeners(string eventName)
        {
            return _retainedCallbacks.HasCallbacks(eventName);
        }

        /// <summary>
        /// Get the value of a named property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="returnValue"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public bool GetProperty(string name, out CefV8Value returnValue, out string exception)
        {
            exception = null;
            returnValue = null;

            if (string.IsNullOrEmpty(name))
            {
                exception = "No property name specified";
                return false;
            }

            if (_expandoProperties.TryGetValue(name, out returnValue))
            {
                return true;
            }

            try
            {
                object val;
                if (OnGetProperty(name, out val))
                {
                    returnValue = CefNativeValueConverter.ToCef(val);
                    return true;
                }

                exception = "Property not found: " + name;
                returnValue = CefV8Value.CreateUndefined();
                return false;
            }
            catch (Exception ex)
            {
                exception = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Set the value of a named property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public bool SetProperty(string name, CefV8Value value, out string exception)
        {
            exception = null;

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            try
            {
                return OnSetProperty(name, value)
                       || _expandoProperties.TryAdd(name, value);
            }
            catch (Exception ex)
            {
                exception = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Execute a native function.
        /// </summary>
        /// <param name="context">The current V8 JavaScript context.</param>
        /// <param name="functionName">The name of the native function to invoke.</param>
        /// <param name="parameters">The parameters to pass to the function.</param>
        /// <param name="callback">An optional callback for the native function to invoke when it is complete.</param>
        public void ExecuteFunction(CefV8Context context, string functionName,
            CefV8Value[] parameters, CefV8Value callback, out CefV8Value returnValue, out string exception)
        {
            Logger.Debug(fmt => fmt("ExecuteFunction Plugin {0} Method {1}", Descriptor.PluginId, functionName));

            MethodDescriptor methodDescriptor;
            if (string.IsNullOrEmpty(functionName)
                || (methodDescriptor = Descriptor.Methods.Find(descriptor => descriptor.MethodName == functionName)) == null)
            {
                throw new MissingMethodException(functionName);
            }

            V8Callback v8Callback = null;
            if (callback != null)
            {
                // Native plugin methods that accept callbacks may include 'unsubscribe' patterns where the same callback
                // is reused or needs to be passed (wrapped as a C# delegate) to allow the unsubsribe.
                v8Callback = methodDescriptor.HasCallbackParameter
                    ? _retainedCallbacks.GetOrCreateCallback(this, "__PARAMETERCALLBACK", callback, V8CallbackType.ParameterCallback)
                    : new V8Callback(this, callback, V8CallbackType.FunctionCallback);
            }

            // Execute the call through the router, so that the router keeps track of the call
            OnInvokeFunction(context, methodDescriptor, parameters, v8Callback, out returnValue, out exception);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var properties = _expandoProperties.Values.ToList();
                _expandoProperties.Clear();
                properties.ForEach(p => p.Dispose());
                _retainedCallbacks.Clear();
            }
        }

        protected abstract void OnAddEventListener(CefV8Context context, string eventName, V8Callback v8Callback);

        protected abstract void OnRemoveEventListener(CefV8Context context, string eventName, V8Callback v8Callback);

        protected virtual bool OnGetProperty(string name, out object value)
        {
            value = null;
            return false;
        }

        protected virtual bool OnSetProperty(string name, CefV8Value value)
        {
            return false;
        }

        protected abstract void OnInvokeFunction(CefV8Context context, MethodDescriptor methodDescriptor, CefV8Value[] parameters, V8Callback v8Callback, out CefV8Value returnValue, out string exception);
    }
}