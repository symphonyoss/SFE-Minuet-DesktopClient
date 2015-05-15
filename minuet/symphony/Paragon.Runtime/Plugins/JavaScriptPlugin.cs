using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Paragon.Plugins;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// A wrapper for a JavaScript Plugin object
    /// </summary>
    public sealed class JavaScriptPlugin : IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly CallbackThread _callbackThread;
        private readonly EventInfo _disposedEvent;
        private readonly ConcurrentDictionary<string, JavaScriptEvent> _events;
        private readonly bool _isDynamic;
        private readonly ConcurrentDictionary<string, MethodInfo> _methods;
        private readonly string _pluginId;
        private readonly ConcurrentDictionary<string, PropertyInfo> _properties;
        private volatile bool _disposed;
        private JavaScriptPluginCallback _disposedHandler;

        private JavaScriptPlugin(PluginProcess pluginProcess, object plugin, JavaScriptPluginAttribute pluginAttribute)
        {
            NativeObject = plugin;
            _pluginId = pluginAttribute.Name;
            _callbackThread = pluginAttribute.CallbackThread;

            if (string.IsNullOrEmpty(_pluginId))
            {
                _isDynamic = true;
                _pluginId = Guid.NewGuid().ToString();
            }

            var pluginType = NativeObject.GetType();

            _methods = new ConcurrentDictionary<string, MethodInfo>(
                GetParagonMembers(pluginType.GetMethods));

            _events = new ConcurrentDictionary<string, JavaScriptEvent>(GetParagonMembers(pluginType.GetEvents)
                .Where(m => m.Value.EventHandlerType == typeof(JavaScriptPluginCallback))
                .ToDictionary(m => m.Key, m => new JavaScriptEvent(NativeObject, m.Value)));

            _properties = new ConcurrentDictionary<string, PropertyInfo>(GetParagonMembers(pluginType.GetProperties));

            if (_isDynamic)
            {
                var disposedEvent = pluginType
                    .GetEvents(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(e =>
                    {
                        var a = e.GetCustomAttributes(typeof(JavaScriptDisposeAttribute), true);
                        return a.Length > 0;
                    });

                if (disposedEvent != null
                    && disposedEvent.EventHandlerType == typeof(JavaScriptPluginCallback))
                {
                    _disposedEvent = disposedEvent;
                    _disposedHandler = OnNativeObjectDisposed;
                    _disposedEvent.AddEventHandler(NativeObject, _disposedHandler);
                }
            }

            foreach (var nativeEvent in _events.Values)
            {
                nativeEvent.AttachToEvent();
            }

            Descriptor = new PluginDescriptor
            {
                PluginId = _pluginId,
                Methods = _methods.Select(method => new MethodDescriptor
                {
                    MethodName = method.Key,
                    HasCallbackParameter = HasCallbackParameter(method.Value),
                    IsVoid = method.Value.ReturnType == typeof(void)
                }).ToList(),
                Events = _events.Keys.ToList()
            };
        }

        public PluginDescriptor Descriptor { get; private set; }

        public bool IsValid
        {
            get { return !_disposed && (_methods.Count > 0 || _properties.Count > 0 || _events.Count > 0); }
        }

        public bool IsDynamic
        {
            get { return _isDynamic; }
        }

        internal object NativeObject { get; private set; }

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
            if (_disposedEvent != null && _disposedHandler != null)
            {
                _disposedEvent.RemoveEventHandler(NativeObject, _disposedHandler);
            }

            NativeObject = null;
            _disposedHandler = null;
            _methods.Clear();
            _properties.Clear();

            var events = _events.Values.ToList();
            _events.Clear();
            events.ForEach(e => e.Dispose());
        }

        public static JavaScriptPlugin CreateFromObject(PluginProcess pluginProcess, object plugin)
        {
            if (plugin == null)
            {
                return null;
            }

            var type = plugin.GetType();
            var pluginAttr = type.GetCustomAttributes(typeof(JavaScriptPluginAttribute), true);
            if (pluginAttr.Length == 0)
            {
                return null;
            }

            return new JavaScriptPlugin(pluginProcess, plugin, pluginAttr[0] as JavaScriptPluginAttribute);
        }

        public static JavaScriptPlugin CreateFromType(PluginProcess pluginProcess, Type type, bool isKernel)
        {
            if (type == null)
            {
                return null;
            }

            var pluginAttr = type.GetCustomAttributes(typeof(JavaScriptPluginAttribute), true);
            if (pluginAttr.Length == 0)
            {
                return null;
            }

            var pAttr = pluginAttr[0] as JavaScriptPluginAttribute;
            if (pAttr == null)
            {
                return null;
            }

            if (isKernel
                && ((pluginProcess == PluginProcess.Browser && !pAttr.IsBrowserSide)
                    || (pluginProcess == PluginProcess.Renderer && pAttr.IsBrowserSide)))
            {
                return null;
            }

            var plugin = new JavaScriptPlugin(pluginProcess, Activator.CreateInstance(type), pAttr);
            return plugin.IsValid ? plugin : null;
        }

        public static bool IsDynamicPlugin(object plugin)
        {
            return plugin != null && IsDynamicPluginType(plugin.GetType());
        }

        public static bool IsDynamicPluginType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            var pluginAttribute = type.GetCustomAttributes(typeof(JavaScriptPluginAttribute), true);
            return pluginAttribute.Length > 0 && string.IsNullOrEmpty(((JavaScriptPluginAttribute)pluginAttribute[0]).Name);
        }

        public event Action<JavaScriptPlugin> DynamicPluginDisposed;

        public void InvokeFunctionDirect(IPluginManager pluginManager, int browserId, long frameId, int contextId,
                                           string methodName, IJavaScriptParameters parameters, IJavaScriptPluginCallback callback)
        {
            var invoker = CreateInvoker(pluginManager, browserId, frameId, contextId, methodName, parameters, callback);
            invoker();
        }

        public void InvokeFunction(
            IPluginManager pluginManager, int browserId, long frameId, int contextId,
            string methodName, IJavaScriptParameters parameters, IJavaScriptPluginCallback callback)
        {
            var invoker = CreateInvoker(pluginManager, browserId, frameId, contextId, methodName, parameters, callback);
            if (_callbackThread == CallbackThread.Main)
            {
                ParagonRuntime.MainThreadContext.Post(o => invoker(), null);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(o => invoker());
            }
        }

        private Action CreateInvoker(IPluginManager pluginManager, int browserId, long frameId, int contextId,
                                       string methodName, IJavaScriptParameters parameters, IJavaScriptPluginCallback callback)
        {
            ThrowIfDisposed();

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            MethodInfo method;
            if (!_methods.TryGetValue(methodName, out method))
            {
                if (callback != null)
                {
                    var msg = string.Format("Error executing {0} on plugin {1} - method not found", methodName, _pluginId);
                    InvokeReturnCallback(callback, null, 0, msg);
                }
            }

            var nativeObject = NativeObject;
            if (nativeObject == null)
            {
                if (callback != null)
                {
                    var msg = string.Format("Error executing {0} on plugin {1} - plugin object has been disposed", methodName, _pluginId);
                    InvokeReturnCallback(callback, null, 0, msg);
                }
            }

            object[] arguments;
            var hasCallbackParameter = HasCallbackParameter(method);
            var methodParams = method.GetParameters();

            var parameterDefinitions = hasCallbackParameter
                ? methodParams.Take(methodParams.Length - 1).ToArray()
                : methodParams;

            try
            {
                arguments = parameters.GetConvertedParameters(parameterDefinitions, pluginManager);

                if (hasCallbackParameter)
                {
                    // Create a new args array with length + 1 so we can add the callback param to it.
                    var args = new object[arguments.Length + 1];
                    Array.Copy(arguments, args, arguments.Length);

                    // Extract the callback and wrap it.
                    JavaScriptPluginCallback callbackParam = null;
                    if (callback != null)
                    {
                        var parameterCallback = callback.GetParameterCallback();
                        if (parameterCallback != null)
                        {
                            callbackParam = parameterCallback.Invoke;
                        }
                    }

                    // Add the wrapped callback to the args list.
                    args[args.Length - 1] = callbackParam;
                    arguments = args;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(fmt => fmt("Error converting plugin invocation parameters: " + ex));
                if (callback != null)
                {
                    InvokeReturnCallback(callback, null, -1, ex.Message);
                }
                return null;
            }

            var invoke = new Action(
                () =>
                {
                    using (PluginExecutionContext.Create(browserId, contextId, frameId))
                    {
                        object result = null;
                        var errorCode = 0;
                        var error = string.Empty;

                        try
                        {
                            result = method.Invoke(nativeObject, arguments);
                        }
                        catch (Exception e)
                        {
                            while (e is TargetInvocationException)
                            {
                                e = e.InnerException;
                            }

                            errorCode = -1;
                            error = e.Message;
                            Logger.Error(fmt => fmt("Error executing plugin method: " + e));
                        }

                        if (callback != null)
                        {
                            InvokeReturnCallback(callback, result, errorCode, error);
                        }
                    }
                });
            return invoke;
        }

        public bool AddEventListener(string eventName, IJavaScriptPluginCallback eventCallback)
        {
            ThrowIfDisposed();
            JavaScriptEvent eventSubscription;
            if (_events.TryGetValue(eventName, out eventSubscription))
            {
                eventSubscription.AddHandler(eventCallback);
                return true;
            }

            return false;
        }

        public void RemoveEventListener(string eventName, IJavaScriptPluginCallback eventCallback)
        {
            ThrowIfDisposed();
            JavaScriptEvent eventSubscription;
            if (_events.TryGetValue(eventName, out eventSubscription))
            {
                eventSubscription.RemoveHandler(eventCallback);
            }
        }

        public bool SetProperty(string name, IJavaScriptValue value)
        {
            ThrowIfDisposed();
            PropertyInfo property;
            var nativeObject = NativeObject;
            if (nativeObject == null || !_properties.TryGetValue(name, out property))
            {
                return false;
            }

            var convertedValue = value.GetConvertedValue(property);
            property.SetValue(nativeObject, convertedValue, null);
            return true;
        }

        public object GetProperty(string name)
        {
            ThrowIfDisposed();
            var nativeObject = NativeObject;
            PropertyInfo property;
            if (nativeObject == null || !_properties.TryGetValue(name, out property))
            {
                return null;
            }

            return property.GetValue(nativeObject, null);
        }

        private static bool HasCallbackParameter(MethodInfo method)
        {
            var lastParameter = method.GetParameters().LastOrDefault();
            return lastParameter != null && lastParameter.ParameterType == typeof(JavaScriptPluginCallback);
        }

        private static IEnumerable<KeyValuePair<string, TMemberInfo>> GetParagonMembers<TMemberInfo>(
            Func<BindingFlags, TMemberInfo[]> getMembers)
            where TMemberInfo : MemberInfo
        {
            Func<MemberInfo, JavaScriptPluginMemberAttribute> getMetadata = mem =>
            {
                var attr = mem.GetCustomAttributes(typeof(JavaScriptPluginMemberAttribute), true);
                return attr.Length > 0 ? attr[0] as JavaScriptPluginMemberAttribute : null;
            };

            return getMembers(BindingFlags.Public | BindingFlags.Instance)
                .Select(member => new { Member = member, Metadata = getMetadata(member) })
                .Where(m => m.Metadata != null)
                .Select(member => new KeyValuePair<string, TMemberInfo>(
                    !string.IsNullOrEmpty(member.Metadata.Name) ? member.Metadata.Name : ToCamelCase(member.Member.Name),
                    member.Member));
        }

        private static void InvokeReturnCallback(IJavaScriptPluginCallback callback, object data, int errorCode, string error)
        {
            try
            {
                callback.Invoke(data, errorCode, error);
            }
            catch (Exception e)
            {
                Logger.Error(fmt => fmt("Error invoking plugin return callback: " + e));
            }
        }

        private static string ToCamelCase(string text)
        {
            if (text.Length > 1)
            {
                return char.ToLower(text[0]) + text.Substring(1);
            }

            return text;
        }

        private void OnNativeObjectDisposed(params object[] args)
        {
            // _nativeObject is a dynamic plugin and is signalling this wrapper that it should be discarded.
            // Detach from the object and signal the runtime that this wrapper should be discarded as well.

            Dispose();
            var dynamicPluginDisposed = DynamicPluginDisposed;
            if (dynamicPluginDisposed != null)
            {
                dynamicPluginDisposed(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}