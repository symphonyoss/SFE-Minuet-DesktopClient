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
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using Paragon.Runtime.Plugins.TypeConversion;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public class V8Callback : IV8Callback
    {
        private static ILogger Logger = ParagonLogManager.GetLogger();
        private readonly V8CallbackType _callbackType;
        private readonly IV8Plugin _plugin;
        private Action<V8Callback> _disposeHandler;
        private bool _disposed;

        public V8Callback(IV8Plugin plugin, CefV8Value callback, V8CallbackType callbackType, Action<V8Callback> disposeHandler = null)
        {
            _plugin = plugin;
            CallbackFunction = callback;
            _callbackType = callbackType;
            _disposeHandler = disposeHandler;
            Identifier = Guid.NewGuid();
        }

        public CefV8Value CallbackFunction { get; private set; }

        public Guid Identifier { get; private set; }

        public virtual void Invoke(IV8PluginRouter router, CefV8Context context, object result, int errorCode, string error)
        {
            if (CallbackFunction == null)
            {
                throw new ObjectDisposedException("_callback");
            }

            // Have to enter the context in order to be able to create object/array/function/date V8 instances
            context.Enter();
            try
            {
                var args = new List<CefV8Value>();
                switch (_callbackType)
                {
                    case V8CallbackType.ParameterCallback:
                    case V8CallbackType.EventListener:
                    {
                        var remoteResult = result as ResultData;
                        var localArray = result as object[];

                        if (remoteResult != null)
                        {
                            if (remoteResult.Items != null)
                            {
                                args.AddRange(remoteResult.Items.Select(item => ToCefV8Value(router, item)));
                            }
                        }
                        else if (localArray != null)
                        {
                            args.AddRange(localArray.Select(item => ToCefV8Value(router, item)));
                        }

                        break;
                    }
                    case V8CallbackType.FunctionCallback:
                    {
                        args.Add(ToCefV8Value(router, result));
                        args.Add(CefV8Value.CreateInt(errorCode));
                        args.Add(CefV8Value.CreateString(error));
                        break;
                    }
                }
                var functionResult = CallbackFunction.ExecuteFunction(null, args.ToArray());
                if (functionResult == null && CallbackFunction.HasException)
                {
                    var exception = CallbackFunction.GetException();
                    Logger.Error("Error executing callback: ", exception.Message);
                }
            }
            finally
            {
                context.Exit();
            }
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
            if (CallbackFunction != null)
            {
                CallbackFunction.Dispose();
                CallbackFunction = null;
            }

            if (_disposeHandler != null)
            {
                _disposeHandler(this);
                _disposeHandler = null;
            }
        }

        protected CefV8Value ToCefV8Value(IV8PluginRouter router, object result)
        {
            if (result == null)
            {
                return CefV8Value.CreateNull();
            }

            // VALUES FROM REMOTE PLUGINS
            var remoteResult = result as ResultData;
            if (remoteResult != null)
            {
                switch (remoteResult.DataType)
                {
                    case ResultDataType.Scalar:
                        if (remoteResult.Items != null && remoteResult.Items.Count != 0)
                        {
                            return ToCefV8Value(router, remoteResult.Items[0]);
                        }
                        return CefV8Value.CreateNull();
                    case ResultDataType.Array:
                    {
                        var cefArray = CefV8Value.CreateArray(remoteResult.Items.Count);
                        if (remoteResult.Items != null)
                        {
                            for (var resultIndex = 0; resultIndex < remoteResult.Items.Count; ++resultIndex)
                            {
                                var cefValue = ToCefV8Value(router, remoteResult.Items[resultIndex]);
                                cefArray.SetValue(resultIndex, cefValue);
                            }
                        }
                        return cefArray;
                    }
                    case ResultDataType.Dictionary:
                    {
                        var cefObject = CefV8Value.CreateObject(null);
                        if (remoteResult.Items != null)
                        {
                            foreach (var dictionaryItem in remoteResult.Items)
                            {
                                if (string.IsNullOrEmpty(dictionaryItem.Name))
                                {
                                    continue;
                                }
                                var cefValue = ToCefV8Value(router, dictionaryItem);
                                cefObject.SetValue(dictionaryItem.Name, cefValue, CefV8PropertyAttribute.None);
                            }
                        }
                        return cefObject;
                    }
                }
            }
            var resultItem = result as ResultItem;
            if (resultItem != null)
            {
                return ToCefV8Value(router, (object) resultItem.DynamicPlugin ?? resultItem.PlainData);
            }
            var pluginObjectDescriptor = result as PluginDescriptor;
            if (pluginObjectDescriptor != null)
            {
                return V8PluginAdapter.CreateRemote(router, _plugin.PluginContext, pluginObjectDescriptor).V8Object;
            }

            // VALUES FROM REMOTE OR LOCAL PLUGINS
            var plainData = result as JToken;
            if (plainData != null)
            {
                return CefJsonValueConverter.ToCef(plainData);
            }

            // VALUES FROM LOCAL PLUGINS
            var localArray = result as object[];
            if (localArray != null)
            {
                var cefArray = CefV8Value.CreateArray(localArray.Length);
                for (var resultIndex = 0; resultIndex < localArray.Length; ++resultIndex)
                {
                    var cefValue = ToCefV8Value(router, localArray[resultIndex]);
                    cefArray.SetValue(resultIndex, cefValue);
                }
                return cefArray;
            }
            var localPlugin = result as JavaScriptPlugin;
            if (localPlugin != null)
            {
                return V8PluginAdapter.CreateLocal(router, _plugin.PluginContext, localPlugin).V8Object;
            }
            if (JavaScriptPlugin.IsDynamicPlugin(result))
            {
                var dynPlugin = JavaScriptPlugin.CreateFromObject(PluginProcess.Renderer, result);
                _plugin.PluginContext.PluginManager.AddLocalPlugin(dynPlugin);
                return V8PluginAdapter.CreateLocal(router, _plugin.PluginContext, dynPlugin).V8Object;
            }

            // local C# POCO
            return CefNativeValueConverter.ToCef(result);
        }
    }
}