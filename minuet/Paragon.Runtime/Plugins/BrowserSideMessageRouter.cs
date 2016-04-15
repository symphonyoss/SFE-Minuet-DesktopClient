//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public sealed class BrowserSideMessageRouter : MessageRouter, IBrowserSideMessageRouter
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly List<DynamicPluginRef> _dynamicPluginRefs = new List<DynamicPluginRef>();
        private readonly CallInfoCollection<BrowserCallInfo> _pendingCallbacks = new CallInfoCollection<BrowserCallInfo>();
        private readonly Guid _pluginGroupId = Guid.NewGuid();
        private readonly IPluginManager _pluginManager;

        public BrowserSideMessageRouter()
            : base(CefProcessId.Browser)
        {
            _pluginManager = new PluginManager(PluginProcess.Browser);
            _pluginManager.LocalPluginRemoved += OnPluginRemoved;
        }

        public IPluginManager PluginManager
        {
            get { return _pluginManager; }
        }

        public CefListValue CreatePluginInitMessage()
        {
            Logger.Info("Creating browser initialization message");
            var initializeMessage = new PluginMessage
            {
                MessageType = PluginMessageType.InitializePlugins,
                MessageId = _pluginGroupId,
                BrowserId = 0
            };

            var pluginGroup = new PluginGroup
            {
                PluginDescriptors = PluginManager.GetAllLocalPlugins().Select(p => p.Descriptor).ToList()
            };

            initializeMessage.Data = JsonConvert.SerializeObject(pluginGroup);
            var retVal = CefListValue.Create();
            SerializeProcessMessage(initializeMessage, retVal);
            return retVal;
        }

        /// <summary>
        /// Send a response to a function call.
        /// Can be called on any thread as the CEF API involved (<see cref="CefBrowser.SendProcessMessage"/>) can be called on any thread when in the browser process.
        /// </summary>
        /// <param name="info">
        /// The callback that was created when the function was invoked.
        /// </param>
        /// <param name="result">
        /// The result of the function (result, errorCode, error).
        /// </param>
        public void SendFunctionResponse(BrowserCallInfo info, ResultData result)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (!info.IsRetained)
            {
                _pendingCallbacks.Remove(info);
            }

            SendFunctionResponse(info.Browser, info.RequestMessage, result);

            if (!info.IsRetained)
            {
                info.Dispose();
            }
        }

        public PluginDescriptor AddDynamicPlugin(object pluginObject)
        {
            // Remove any dead references that may exist.
            _dynamicPluginRefs.RemoveAll(r => !r.IsAlive);

            // Check if a dynamic plugin has already been created for the specified target.
            var pluginRef = _dynamicPluginRefs.FirstOrDefault(wr => wr.MatchesTarget(pluginObject));
            if (pluginRef != null)
            {
                // Found one, return it's descriptor.
                return pluginRef.Plugin.Descriptor;
            }

            // No existing dynamic plugin found. Create one and store a reference to it.
            var dynPlugin = JavaScriptPlugin.CreateFromObject(PluginProcess.Browser, pluginObject);
            pluginRef = new DynamicPluginRef(pluginObject, dynPlugin);
            _dynamicPluginRefs.Add(pluginRef);

            // Add the dynamic plugin to the plugin manager.
            PluginManager.AddLocalPlugin(dynPlugin);
            return dynPlugin.Descriptor;
        }

        /// <summary>
        /// Trigger an event callback in a remote process.
        /// Can be called on any thread as the CEF API involved (<see cref="CefBrowser.SendProcessMessage"/>) can be called on any thread when in the browser process.
        /// </summary>
        /// <param name="listener">
        /// The message that requested to listen to the triggered event.
        /// </param>
        /// <param name="result">
        /// The data payload of the triggered event.
        /// </param>
        public void SendEvent(BrowserCallInfo listener, ResultData result)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            var eventMessage = new PluginMessage
            {
                MessageId = listener.RequestMessage.MessageId,
                MessageType = PluginMessageType.EventFired,
                PluginId = string.Empty,
                MemberName = string.Empty,
                Data = ParagonJsonSerializer.Serialize(result),
                BrowserId = listener.RequestMessage.BrowserId,
                ContextId = listener.RequestMessage.ContextId
            };

            SendMessage(listener.Browser, eventMessage);
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
            }
        }

        protected override void ReceiveMessage(CefBrowser browser, PluginMessage pluginMessage)
        {
            var handler = PluginManager.GetLocalPlugin(pluginMessage.PluginId);
            if (handler == null)
            {
                // No handlers so cancel the query.
                if (pluginMessage.MessageType == PluginMessageType.FunctionInvoke)
                {
                    CancelUnhandledQuery(browser, pluginMessage);
                }
                browser.Dispose();
                return;
            }

            switch (pluginMessage.MessageType)
            {
                case PluginMessageType.FunctionInvoke:
                    OnInvokeFunction(browser, pluginMessage, handler);
                    break;
                case PluginMessageType.AddListener:
                    OnAddListener(browser, pluginMessage, handler);
                    break;
                case PluginMessageType.RemoveRetained:
                    OnRemoveRetained(browser, pluginMessage, handler);
                    break;
            }
        }

        private void OnPluginRemoved(JavaScriptPlugin handler)
        {
            if (handler == null)
            {
                return;
            }

            // Only remove event listeners and parameter callbacks - locking inside JavaScriptPlugin means any 
            // inflight calls will still complete, so leave those to be removed and disposed as per usual
            var retainedCallbacksAndListeners = _pendingCallbacks.RemoveWhere(info =>
                info.PluginId == handler.Descriptor.PluginId && info.IsRetained);

            foreach (var eventListener in retainedCallbacksAndListeners)
            {
                // Tell the remote plugin process that owns the listener or parameter callbacks to dispose of it
                var removeListenerMessage = new PluginMessage
                {
                    MessageId = eventListener.RequestMessage.MessageId,
                    MessageType = PluginMessageType.RemoveRetained,
                    PluginId = eventListener.RequestMessage.PluginId,
                    MemberName = eventListener.RequestMessage.MemberName,
                    Data = string.Empty,
                    BrowserId = eventListener.RequestMessage.BrowserId,
                    ContextId = eventListener.RequestMessage.ContextId
                };

                SendMessage(eventListener.Browser, removeListenerMessage);
                eventListener.Dispose();
            }
        }

        private void OnInvokeFunction(CefBrowser browser, PluginMessage pluginMessage, JavaScriptPlugin handler)
        {
            var methodDescriptor = handler.Descriptor.Methods.Find(descriptor => descriptor.MethodName == pluginMessage.MemberName);
            IJavaScriptPluginCallback returnCallback = null;
            BrowserCallInfo parameterCallback = null;

            if (pluginMessage.V8CallbackId != Guid.Empty)
            {
                if (methodDescriptor.HasCallbackParameter)
                {
                    // Create a second stored callback info which represents the V8 callback function itself
                    // rather than the method that is being invoked now. This allows the callback function
                    // to be passed to and invoked by multiple native methods that accept a callback parameter.
                    parameterCallback = _pendingCallbacks.Get(pluginMessage.V8CallbackId);
                    if (parameterCallback == null)
                    {
                        var parameterCallbackMessage = new PluginMessage
                        {
                            MessageId = pluginMessage.V8CallbackId,
                            MessageType = PluginMessageType.ParameterCallback,
                            PluginId = string.Empty,
                            MemberName = string.Empty,
                            BrowserId = pluginMessage.BrowserId,
                            ContextId = pluginMessage.ContextId,
                            FrameId = pluginMessage.FrameId,
                            V8CallbackId = Guid.Empty
                        };

                        parameterCallback = CreateAndAddCall(browser.Clone(), parameterCallbackMessage, null, null);
                    }
                }

                var returnCallInfo = CreateAndAddCall(browser, pluginMessage, handler, parameterCallback);
                if (!handler.IsValid)
                {
                    RemoveAndCancelCall(returnCallInfo);
                    return;
                }

                returnCallback = returnCallInfo;
            }

            JArray callArgs = null;
            if (!string.IsNullOrEmpty(pluginMessage.Data))
            {
                callArgs = JArray.Parse(pluginMessage.Data);
            }

            handler.InvokeFunction(
                _pluginManager,
                pluginMessage.BrowserId,
                pluginMessage.FrameId,
                pluginMessage.ContextId,
                pluginMessage.MemberName,
                new JArrayJavaScriptParameters(callArgs),
                returnCallback);
        }

        private void OnAddListener(CefBrowser browser, PluginMessage pluginMessage, JavaScriptPlugin handler)
        {
            var info = CreateAndAddCall(browser, pluginMessage, handler, null);
            if (!handler.IsValid)
            {
                RemoveAndCancelCall(info);
                return;
            }

            var handled = handler.AddEventListener(pluginMessage.MemberName, info);
            if (!handled)
            {
                RemoveAndCancelCall(info);
            }
        }

        private void OnRemoveRetained(CefBrowser browser, PluginMessage pluginMessage, JavaScriptPlugin handler)
        {
            var info = _pendingCallbacks.Remove(pluginMessage.MessageId);
            if (info != null)
            {
                if (info.IsEventListener)
                {
                    handler.RemoveEventListener(pluginMessage.MemberName, info);
                }

                info.Dispose();
            }

            browser.Dispose();
        }

        private BrowserCallInfo CreateAndAddCall(CefBrowser browser, PluginMessage pluginMessage,
            JavaScriptPlugin handler, IJavaScriptParameterCallback parameterCallback)
        {
            var pluginId = handler != null ? handler.Descriptor.PluginId : null;
            var info = new BrowserCallInfo(this, browser, pluginMessage, pluginId, parameterCallback);
            _pendingCallbacks.Add(info);
            return info;
        }

        private void RemoveAndCancelCall(BrowserCallInfo info)
        {
            _pendingCallbacks.Remove(info);

            if (info.RequestMessage.MessageType == PluginMessageType.FunctionCallback)
            {
                CancelUnhandledQuery(info.Browser, info.RequestMessage);
            }

            info.Dispose();
        }

        /// <summary>
        /// Respond to a function invocation with the result that the function wasn't handled.
        /// Can be called on any thread as the CEF API involved (<see cref="CefBrowser.SendProcessMessage"/>) can be called on any thread when in the browser process.
        /// </summary>
        /// <param name="browser">
        /// The browser to send the cancellation to.
        /// </param>
        /// <param name="pluginMessage">
        /// The function invocation request message.
        /// </param>
        private void CancelUnhandledQuery(CefBrowser browser, PluginMessage pluginMessage)
        {
            var result = new ResultData
            {
                DataType = ResultDataType.Scalar,
                ErrorCode = CallInfo.ErrorCodeCallCanceled,
                Error = CallInfo.ErrorCallCanceled
            };

            SendFunctionResponse(browser, pluginMessage, result);
        }

        /// <summary>
        /// Send a response to a function call, which can include the call being cancelled.
        /// Can be called on any thread as the CEF API involved (<see cref="CefBrowser.SendProcessMessage"/>) can be called on any thread when in the browser process.
        /// </summary>
        /// <param name="browser">
        /// The browser to send the response to.
        /// </param>
        /// <param name="invocationMessage">
        /// The message that requested the function call.
        /// </param>
        /// <param name="result">
        /// The result of the function (result, errorCode, error).
        /// </param>
        private void SendFunctionResponse(CefBrowser browser, PluginMessage invocationMessage, ResultData result)
        {
            var responseMessage = new PluginMessage
            {
                MessageId = invocationMessage.MessageId,
                MessageType = PluginMessageType.FunctionCallback,
                PluginId = invocationMessage.PluginId,
                MemberName = invocationMessage.MemberName,
                Data = ParagonJsonSerializer.Serialize(result),
                BrowserId = invocationMessage.BrowserId,
                ContextId = invocationMessage.ContextId
            };

            SendMessage(browser, responseMessage);
        }

        private class DynamicPluginRef
        {
            private readonly WeakReference _targetRef;
            private readonly WeakReference _pluginRef;

            public DynamicPluginRef(object target, JavaScriptPlugin plugin)
            {
                _targetRef = new WeakReference(target);
                _pluginRef = new WeakReference(plugin);
            }

            public bool IsAlive
            {
                get { return _targetRef.IsAlive; }
            }

            public JavaScriptPlugin Plugin
            {
                get { return IsAlive ? (JavaScriptPlugin) _pluginRef.Target : null; }
            }

            public bool MatchesTarget(object target)
            {
                return _targetRef.Target == target;
            }
        }
    }
}