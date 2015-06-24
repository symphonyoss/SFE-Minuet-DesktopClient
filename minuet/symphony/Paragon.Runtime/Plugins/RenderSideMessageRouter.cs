using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public sealed class RenderSideMessageRouter : MessageRouter, IRenderSideMessageRouter, IV8PluginRouter
    {
        private const int ReservedId = 0;
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly Dictionary<int, CefV8Context> _contexts = new Dictionary<int, CefV8Context>();
        private readonly CallInfoCollection<RenderCallInfo> _pendingCallbacks = new CallInfoCollection<RenderCallInfo>();
        private int _nextContextId;
        private IPluginContext _pluginContext;
        public event Action<IPluginContext> OnPluginContextCreated;

        public RenderSideMessageRouter()
            : base(CefProcessId.Renderer)
        {
        }

        public void BrowserDestroyed(CefBrowser browser)
        {
            Logger.Info("BrowserDestroyed Browser {0}", browser.Identifier);
        }

        public void ContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            Logger.Info("ContextCreated Browser {0} Frame {1}", browser.Identifier, frame.Identifier);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            try
            {
                CreateIdForContext(context);

                InitializeContext(browser, frame, context);
            }
            catch (Exception exception)
            {
                Logger.Info("ContextCreated Failed Browser {0} Frame {1}: {2}", browser.Identifier, frame.Identifier, exception);
            }
        }

        public void ContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            Logger.Info("ContextReleased Browser {0} Frame {1}", browser.Identifier, frame.Identifier);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            try
            {
                // Get the context ID and remove the context from the map.
                var contextId = GetIdForContext(context, true);
                if (contextId != 0)
                {
                    var pending = _pendingCallbacks.RemoveWhere(call => call.ContextId == contextId);
                    foreach (var pendingCall in pending)
                    {
                        pendingCall.Dispose();
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Info("ContextReleased Failed Browser {0} Frame {1}: {2}", browser.Identifier, frame.Identifier, exception);
            }
        }

        /// <summary>
        /// Calls back into V8 in response to either:
        /// - a local function response message 
        /// - a local event fired message
        /// - an exception while making a local function call
        /// Will switch to the Renderer thread if needed.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="result"></param>
        /// <param name="errorCode"></param>
        /// <param name="error"></param>
        public void LocalCallbackInvoked(LocalRenderCallInfo call, object result, int errorCode, string error)
        {
            if (!CefRuntime.CurrentlyOn(CefThreadId.Renderer))
            {
                // Call completed received from remote process, so currently are on the IO method. Marshal to the renderer to callback into V8
                CefRuntime.PostTask(CefThreadId.Renderer,
                    new ActionCallbackTask(() => LocalCallbackInvoked(call, result, errorCode, error)));
                return;
            }

            var msg = call.RequestMessage;

            Logger.Info("LocalCallbackInvoked MsgType {0} Plugin {1} Member {2}",
                msg.MessageType, msg.PluginId, msg.MemberName);

            // Already on the render thread (e.g. local plugin event triggered by call into function from JS, or function invoke failed
            try
            {
                var context = GetContextById(call.RequestMessage.ContextId);
                if (!call.IsRetained)
                {
                    _pendingCallbacks.Remove(call);
                }
                if (context != null && call.Callback != null)
                {
                    call.Callback.Invoke(this, context, result, errorCode, error);
                }
                if (!call.IsRetained)
                {
                    call.Dispose();
                }
            }
            catch (Exception exception)
            {
                Logger.Error("LocalCallbackInvoked Failed MsgType {0} Plugin {1} Member {2}: {3}",
                    call.RequestMessage.MessageType,
                    call.RequestMessage.PluginId,
                    call.RequestMessage.MemberName,
                    exception);
            }
        }

        public void InitializePlugins(CefListValue browserPlugins)
        {
            InitializePluginContext(DeserializeProcessMessage(browserPlugins));
        }

        /// <summary>
        /// Invoke a remote function.
        /// </summary>
        /// <param name="context">
        /// The current V8 context that is making the function call.
        /// </param>
        /// <param name="targetPlugin">
        /// The remote plugin that owns the target method.
        /// </param>
        /// <param name="methodDescriptor">
        /// The method to invoke.
        /// </param>
        /// <param name="parameters">
        /// The parameters to pass to the remote function.
        /// </param>
        /// <param name="callback">
        /// Optional callback into V8.
        /// </param>
        public void InvokeFunction(
            CefV8Context context,
            PluginDescriptor targetPlugin,
            MethodDescriptor methodDescriptor,
            JArray parameters,
            IV8Callback callback)
        {
            Logger.Debug("InvokeFunction Remote Plugin {0} Method {1}", targetPlugin.PluginId, methodDescriptor.MethodName);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            GetOrCreateParameterCallback(context, methodDescriptor, callback);

            var functionInvokeMessage = CreateMessage(context, targetPlugin, methodDescriptor.MethodName, callback);
            functionInvokeMessage.MessageId = Guid.NewGuid();
            functionInvokeMessage.MessageType = PluginMessageType.FunctionInvoke;
            functionInvokeMessage.Data = parameters != null ? parameters.ToString() : string.Empty;

            // Add the call info into the pending calls for the browser if the return type
            // is not void or the method has a callback arg.
            // TODO: We should verify that the signature of the JavaScript call matches the plugin method and throw an exception for invalid invocations.
            if (!methodDescriptor.IsVoid 
                || methodDescriptor.HasCallbackParameter
                || callback != null)
            {
                AddRemoteCallback(functionInvokeMessage, methodDescriptor.HasCallbackParameter ? null : callback);
            }

            try
            {
                using (var browser = context.GetBrowser())
                {
                    // Send the request message to the browser
                    SendMessage(browser, functionInvokeMessage);
                }
            }
            catch (Exception ex)
            {
                // If the request could not be sent, remove the call from the list
                var error = new ResultData {ErrorCode = -1, Error = ex.Message};
                OnBrowserCallbackInvokeReceived(functionInvokeMessage, error);
                Logger.Error("InvokeFunction Failed Remote Plugin {0} Method {1}: {2}",
                    targetPlugin.PluginId,
                    methodDescriptor.MethodName,
                    ex);
            }
        }

        /// <summary>
        /// Add a local V8 callback to a remote event as a listener.
        /// </summary>
        /// <param name="context">
        /// The current V8 context that is adding the listener.
        /// </param>
        /// <param name="targetPlugin">
        /// The remote plugin that owns the event.
        /// </param>
        /// <param name="eventName">
        /// The name of the event to attach to.
        /// </param>
        /// <param name="callback">
        /// The callback to V8 to invoke when the remote event is raised.
        /// </param>
        public void AddEventListener(CefV8Context context, PluginDescriptor targetPlugin, string eventName, IV8Callback callback)
        {
            Logger.Info("AddEventListener Remote Plugin {0} Event {1}", targetPlugin.PluginId, eventName);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            var addListenerMessage = CreateMessage(context, targetPlugin, eventName, callback);
            // Use identifier from callback as ID for listener message.
            // When remote event fired, renderer will receive an EventFired message with this same ID as its MessageId.
            addListenerMessage.MessageId = callback.Identifier;
            addListenerMessage.MessageType = PluginMessageType.AddListener;
            addListenerMessage.Data = string.Empty;

            // Add the call info into the pending calls for the browser
            var info = AddRemoteCallback(addListenerMessage, callback);

            // Send the request message to the browser
            try
            {
                using (var browser = context.GetBrowser())
                {
                    SendMessage(browser, addListenerMessage);
                }
            }
            catch (Exception ex)
            {
                // If the request could not be sent, remove the call from the list
                _pendingCallbacks.Remove(info);
                info.Dispose();
                Logger.Error("AddEventListener Failed Remote Plugin {0} Event {1}: {2}", targetPlugin.PluginId, eventName, ex);
            }
        }

        /// <summary>
        /// Remove a local V8 callback from a remote event.
        /// </summary>
        /// <param name="context">
        /// The current V8 context that is removing the listener.
        /// </param>
        /// <param name="targetPlugin">
        /// The remote plugin that owns the event.
        /// </param>
        /// <param name="eventName">
        /// The name of the event to detach from.
        /// </param>
        /// <param name="callback">
        /// The callback to remove.</param>
        public void RemoveEventListener(CefV8Context context, PluginDescriptor targetPlugin, string eventName, IV8Callback callback)
        {
            Logger.Info("RemoveEventListener Remote Plugin {0} Event {1}", targetPlugin.PluginId, eventName);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            var callId = callback.Identifier;
            var info = _pendingCallbacks.Remove(callId);
            info.Dispose();

            var removeListenerMessage = CreateMessage(context, targetPlugin, eventName, callback);
            // Use identifier from callback as ID for listener message.
            // Remote process will find and remove listener based on the MessageId.
            removeListenerMessage.MessageId = callId;
            removeListenerMessage.MessageType = PluginMessageType.RemoveRetained;
            removeListenerMessage.Data = string.Empty;

            // Send the request message to the browser
            try
            {
                using (var browser = context.GetBrowser())
                {
                    SendMessage(browser, removeListenerMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RemoveEventListener Failed Remote Plugin {0} Event {1}: {2}", targetPlugin.PluginId, eventName, ex);
            }
        }

        /// <summary>
        /// Invoke a local function.
        /// </summary>
        /// <param name="context">
        /// The current V8 context that is making the function call.
        /// </param>
        /// <param name="targetPlugin">
        /// The local plugin that owns the target method.
        /// </param>
        /// <param name="methodDescriptor">
        /// The method to invoke.
        /// </param>
        /// <param name="parameters">
        /// An interface for the local plugin to obtain the parameters from before invoking the function.
        /// </param>
        /// <param name="callback">
        /// Optional callback into V8.
        /// </param>
        public void InvokeFunction(
            CefV8Context context,
            JavaScriptPlugin targetPlugin,
            MethodDescriptor methodDescriptor,
            IJavaScriptParameters parameters,
            IV8Callback callback)
        {
            Logger.Info("InvokeFunction Local Plugin {0} Method {1}", targetPlugin.Descriptor.PluginId, methodDescriptor.MethodName);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            if (!targetPlugin.IsValid)
            {
                Logger.Warn("InvokeFunction Local Plugin {0} is invalid", targetPlugin.Descriptor.PluginId);
                if (callback != null)
                {
                    callback.Invoke(this, context, null, CallInfo.ErrorCodeCallCanceled, CallInfo.ErrorCallCanceled);
                }
                return;
            }

            var parameterCallbackInfo = GetOrCreateParameterCallback(context, methodDescriptor, callback);

            var functionInvokeMessage = CreateMessage(context, targetPlugin.Descriptor, methodDescriptor.MethodName, callback);
            functionInvokeMessage.MessageId = Guid.NewGuid();
            functionInvokeMessage.MessageType = PluginMessageType.FunctionInvoke;

            // Add the call info into the pending calls for the browser
            var info = methodDescriptor.HasCallbackParameter
                ? AddLocalCallback(functionInvokeMessage, null, parameterCallbackInfo)
                : AddLocalCallback(functionInvokeMessage, callback, null);

            try
            {
                targetPlugin.InvokeFunctionDirect(
                    null,
                    functionInvokeMessage.BrowserId,
                    functionInvokeMessage.FrameId,
                    functionInvokeMessage.ContextId,
                    methodDescriptor.MethodName,
                    parameters,
                    info);
            }
            catch (Exception ex)
            {
                LocalCallbackInvoked(info, null, -1, ex.Message);
                Logger.Error("InvokeFunction Failed Local Plugin {0} Method {1}: {2}",
                    targetPlugin.Descriptor.PluginId,
                    methodDescriptor.MethodName,
                    ex);
            }
        }

        /// <summary>
        /// Add a local V8 callback to a local event as a listener.
        /// </summary>
        /// <param name="context">
        /// The current V8 context that is adding the listener.
        /// </param>
        /// <param name="targetPlugin">
        /// The local plugin that owns the event.
        /// </param>
        /// <param name="eventName">
        /// The name of the event to attach to.
        /// </param>
        /// <param name="callback">
        /// The callback to V8 to invoke when the event is raised.
        /// </param>
        public void AddEventListener(CefV8Context context, JavaScriptPlugin targetPlugin, string eventName, IV8Callback callback)
        {
            Logger.Info("AddEventListener Local Plugin {0} Event {1}", targetPlugin.Descriptor.PluginId, eventName);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            if (!targetPlugin.IsValid)
            {
                Logger.Warn("AddEventListener Local Plugin {0} is invalid", targetPlugin.Descriptor.PluginId);
                if (callback != null)
                {
                    callback.Invoke(this, context, null, CallInfo.ErrorCodeCallCanceled, CallInfo.ErrorCallCanceled);
                }
                return;
            }

            var addListenerMessage = CreateMessage(context, targetPlugin.Descriptor, eventName, callback);
            addListenerMessage.MessageId = callback.Identifier;
            addListenerMessage.MessageType = PluginMessageType.AddListener;

            // Add the call info into the pending calls for the browser
            var info = AddLocalCallback(addListenerMessage, callback, null);

            try
            {
                targetPlugin.AddEventListener(eventName, info);
            }
            catch (Exception ex)
            {
                // Remove listener from calls cache
                _pendingCallbacks.Remove(info);
                info.Dispose();
                Logger.Error("AddEventListener Failed Local Plugin {0} Event {1}: {2}", targetPlugin.Descriptor.PluginId, eventName, ex);
            }
        }

        public void RemoveEventListener(JavaScriptPlugin targetPlugin, string eventName,
            IV8Callback callback)
        {
            Logger.Info("RemoveEventListener Local Plugin {0} Event {1}", targetPlugin.Descriptor.PluginId, eventName);

            if (!EnsureOnRendererThread())
            {
                return;
            }

            try
            {
                var info = (LocalRenderCallInfo) _pendingCallbacks.Remove(callback.Identifier);
                info.Dispose();

                targetPlugin.RemoveEventListener(eventName, info);
            }
            catch (Exception exception)
            {
                Logger.Error("RemoveEventListener Failed Local Plugin {0} Event {1}: {2}",
                    targetPlugin.Descriptor.PluginId,
                    eventName,
                    exception);
            }
        }

        protected override void ReceiveMessage(CefBrowser browser, PluginMessage pluginMessage)
        {
            switch (pluginMessage.MessageType)
            {
                case PluginMessageType.FunctionCallback:
                case PluginMessageType.EventFired:
                    var payload = ParagonJsonSerializer.Deserialize<ResultData>(pluginMessage.Data);
                    OnBrowserCallbackInvokeReceived(pluginMessage, payload);
                    break;
                case PluginMessageType.RemoveRetained:
                    // Remove a listener in the render process (handles browser side dynamic plugin dispose scenario)
                    var retainedCallback = _pendingCallbacks.Remove(pluginMessage.MessageId);
                    if (retainedCallback != null)
                    {
                        retainedCallback.Dispose();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            browser.Dispose();
        }

        private void OnPluginRemoved(JavaScriptPlugin handler)
        {
            if (handler == null)
            {
                return;
            }

            Logger.Info("PluginRemoved ID {0}", handler.Descriptor.PluginId);

            // Only remove event listeners and parameter callbacks - locking inside JavaScriptPlugin means any 
            // inflight calls will still complete, so leave those to be removed and disposed as per usual
            var pending = _pendingCallbacks.RemoveWhere(info => info.PluginId == handler.Descriptor.PluginId && info.IsRetained);

            foreach (var retainedCall in pending.OfType<LocalRenderCallInfo>())
            {
                if (retainedCall.IsEventListener)
                {
                    handler.RemoveEventListener(retainedCall.RequestMessage.MemberName, retainedCall);
                }
                retainedCall.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _pendingCallbacks.Dispose();

                if (_pluginContext != null)
                {
                    _pluginContext.Dispose();
                }
                lock (_contexts)
                {
                    foreach (var context in _contexts.Values)
                    {
                        context.Dispose();
                    }

                    _contexts.Clear();
                }
            }
        }

        private void InitializePluginContext(PluginMessage initializeMessage)
        {
            Logger.Info("Initializing Plugin Context");

            try
            {
                if (_pluginContext == null)
                {
                    var pluginContext = new PluginContext(this, initializeMessage.MessageId);
                    var pluginGroup = JsonConvert.DeserializeObject<PluginGroup>(initializeMessage.Data);
                    if (pluginGroup == null)
                    {
                        throw new SerializationException("no plugin group deserialized");
                    }
                    if (this.OnPluginContextCreated != null)
                        this.OnPluginContextCreated(pluginContext);
                    pluginContext.Initialize(pluginGroup);
                    _pluginContext = pluginContext;
                    pluginContext.PluginManager.LocalPluginRemoved += OnPluginRemoved;
                }
            }
            catch (Exception exception)
            {
                Logger.Error("Initializing Plugin Context failed", exception);
            }
        }

        private void AddRenderSidePlugins(IApplicationPackage package, List<IPluginInfo> renderSidePlugins, IPluginContext pluginContext)
        {
            if (renderSidePlugins != null && pluginContext != null )
            {
                foreach (var pluginInfo in renderSidePlugins)
                {
                    try
                    {
                        if (pluginInfo.RunInRenderer &&
                            !pluginInfo.Assembly.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Logger.Info("Creating render-side plugin : " + pluginInfo.Name);
                            var assembly = Assembly.Load(pluginInfo.Assembly);
                            if (assembly != null)
                            {
                                var plugin = pluginContext.PluginManager.AddApplicationPlugin(pluginInfo.Name, assembly) as IParagonPlugin;
                                if (plugin != null)
                                {
                                    plugin.Initialize(null);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Could not create render-side plugin : " + pluginInfo.Name, ex); 
                    }
                }
            }
        }

        private void InitializeContext(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            if (_pluginContext == null)
            {
                Logger.Error("Could not locate plugin context for V8. Browser {0} Frame {1}", browser.Identifier, frame.Identifier);
                return;
            }

            // TODO : Put in an optimization to not create the JS Object model if the current browser is a DEV TOOLS instance
            var window = context.GetGlobal();
            foreach (var pluginId in _pluginContext.GetPluginIds())
            {
                var pluginPath = pluginId.Split('.');

                var pluginV8HostObject = window;
                var clashDetected = false;
                for (var pluginPathSectionIndex = 0; pluginPathSectionIndex < pluginPath.Length; pluginPathSectionIndex++)
                {
                    var pathSection = pluginPath[pluginPathSectionIndex];
                    if (pluginPathSectionIndex == 0 && pathSection == "window")
                    {
                        // Fully scoped path - parent should stay as the root/global object until next section
                        continue;
                    }

                    if (pluginV8HostObject.HasValue(pathSection))
                    {
                        pluginV8HostObject = pluginV8HostObject.GetValue(pathSection);
                        if (!pluginV8HostObject.IsObject)
                        {
                            // Most likely a clash of JS paths between plugins
                            // TODO: warn that this current plugin could not be added
                            clashDetected = true;
                            break;
                        }
                    }
                    else
                    {
                        var child = CefV8Value.CreateObject(null);
                        pluginV8HostObject.SetValue(pathSection, child,
                            CefV8PropertyAttribute.DontEnum |
                            CefV8PropertyAttribute.DontDelete);
                        pluginV8HostObject = child;
                    }
                }
                if (clashDetected)
                {
                    continue;
                }

                V8PluginAdapter.Create(_pluginContext.GetPluginById(pluginId), pluginV8HostObject);
            }
        }

        /// <summary>
        /// Calls back into V8 in response to either:
        /// - a remote function response message (triggered by the framework or by a native plugin calling a delegate it was provided)
        /// - a remote event fired message
        /// - an exception while making a remote function call
        /// Will switch to the Renderer thread if needed.
        /// </summary>
        /// <param name="pluginMessage"></param>
        /// <param name="result"></param>
        private void OnBrowserCallbackInvokeReceived(PluginMessage pluginMessage, ResultData result)
        {
            Logger.Debug("BrowserCallbackInvokeReceived MsgType {0} Plugin {1} Member {2}",
                pluginMessage.MessageType,
                pluginMessage.PluginId,
                pluginMessage.MemberName);

            try
            {
                var context = GetContextById(pluginMessage.ContextId);
                var info = _pendingCallbacks.Get(pluginMessage.MessageId);
                if (info != null)
                {
                    if (!info.IsRetained)
                    {
                        _pendingCallbacks.Remove(info);
                    }

                    // Call completed received from remote process, so currently are on the IO method. Marshal to the renderer to callback into V8
                    CefRuntime.PostTask(CefThreadId.Renderer, new ActionCallbackTask(() => InvokeV8Callback(context, info, result)));
                }
            }
            catch (Exception exception)
            {
                Logger.Error("BrowserCallbackInvokeReceived Failed MsgType {0} Plugin {1} Member {2}: {3}",
                    pluginMessage.MessageType,
                    pluginMessage.PluginId,
                    pluginMessage.MemberName,
                    exception);
            }
        }

        /// <summary>
        /// Called by <see cref="OnBrowserCallbackInvokeReceived(PluginMessage,ResultData)"/>
        /// to invoke a V8 callback.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="info"></param>
        /// <param name="result"></param>
        private void InvokeV8Callback(CefV8Context context, RenderCallInfo info, ResultData result)
        {
            try
            {
                if (context != null && info.Callback != null)
                {
                    info.Callback.Invoke(this, context, result, result.ErrorCode, result.Error);
                }
                if (!info.IsRetained)
                {
                    info.Dispose();
                }
            }
            catch (Exception exception)
            {
                Logger.Error("InvokeV8Callback Failed: {0}", exception);
            }
        }

        private RenderCallInfo AddRemoteCallback(PluginMessage pluginMessage, IV8Callback callback)
        {
            var info = new RenderCallInfo(pluginMessage, callback);
            _pendingCallbacks.Add(info);
            return info;
        }

        private LocalRenderCallInfo AddLocalCallback(PluginMessage pluginMessage, IV8Callback callback, IJavaScriptParameterCallback parameterCallback)
        {
            var info = new LocalRenderCallInfo(this, pluginMessage, callback, parameterCallback);
            _pendingCallbacks.Add(info);
            return info;
        }

        private LocalRenderCallInfo GetOrCreateParameterCallback(CefV8Context context, MethodDescriptor methodDescriptor, IV8Callback callback)
        {
            LocalRenderCallInfo parameterCallbackInfo = null;
            if (methodDescriptor.HasCallbackParameter && callback != null)
            {
                // Create a second stored callback info which represents the V8 callback function itself
                // rather than the method that is being invoked now. This allows the callback function
                // to be passed to and invoked by multiple native methods that accept a callback parameter.
                parameterCallbackInfo = (LocalRenderCallInfo) _pendingCallbacks.Get(callback.Identifier);
                if (parameterCallbackInfo == null)
                {
                    var parameterCallbackMessage = new PluginMessage
                    {
                        MessageId = callback.Identifier,
                        MessageType = PluginMessageType.ParameterCallback,
                        PluginId = string.Empty,
                        MemberName = string.Empty,
                        BrowserId = context.GetBrowserId(),
                        ContextId = GetIdForContext(context, false), 
                        FrameId = context.GetFrame().Identifier,
                        V8CallbackId = Guid.Empty
                    };

                    parameterCallbackInfo = AddLocalCallback(parameterCallbackMessage, callback, null);
                }
            }
            return parameterCallbackInfo;
        }

        private void CreateIdForContext(CefV8Context context)
        {
            // The context should not already have an associated ID.
            if (GetIdForContext(context, false) != ReservedId)
            {
                throw new Exception("Context already exists!");
            }

            lock (_contexts)
            {
                var id = ++_nextContextId;
                _contexts[id] = context;
            }
        }

        /// <summary>
        /// Retrieves the existing ID value associated with the specified <param name="context">.</param>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="remove">If true the context will also be removed from the map.</param>
        /// <returns></returns>
        private int GetIdForContext(CefV8Context context, bool remove)
        {
            lock (_contexts)
            {
                foreach (var kvp in _contexts.Where(kvp => kvp.Value.IsSame(context)))
                {
                    if (remove)
                    {
                        _contexts.Remove(kvp.Key);
                    }
                    return kvp.Key;
                }
            }
            return ReservedId;
        }

        private CefV8Context GetContextById(int contextId)
        {
            lock (_contexts)
            {
                CefV8Context context;
                return _contexts.TryGetValue(contextId, out context) ? context : null;
            }
        }

        private PluginMessage CreateMessage(
            CefV8Context context,
            PluginDescriptor targetPlugin,
            string memberName,
            IV8Callback callback)
        {
            var message = new PluginMessage
            {
                PluginId = targetPlugin.PluginId,
                MemberName = memberName,
                BrowserId = context.GetBrowserId(),
                ContextId = GetIdForContext(context, false),
                FrameId = context.GetFrame().Identifier,
                V8CallbackId = callback != null ? callback.Identifier : Guid.Empty
            };

            return message;
        }

        private static bool EnsureOnRendererThread()
        {
            if (!CefRuntime.CurrentlyOn(CefThreadId.Renderer))
            {
                Logger.Error("Current thread is not the render thread");
                return false;
            }
            return true;
        }
    }
}