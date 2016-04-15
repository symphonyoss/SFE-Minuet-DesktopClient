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
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// A plugin object exposed to V8. This may be a local (loaded into the render process) 
    /// or remote (a proxy for a plugin object loaded into the browser process or a seperate Plugin process) plugin.
    /// </summary>
    public sealed class LocalV8Plugin : V8Plugin
    {
        private readonly JavaScriptPlugin _plugin;
        private readonly IV8PluginRouter _router;

        public LocalV8Plugin(IV8PluginRouter router, IPluginContext pluginContext, JavaScriptPlugin plugin)
            : base(pluginContext, plugin.Descriptor)
        {
            if (router == null)
            {
                throw new ArgumentNullException("router");
            }

            _router = router;
            _plugin = plugin;
        }

        protected override void OnAddEventListener(CefV8Context context, string eventName, V8Callback v8Callback)
        {
            _router.AddEventListener(context, _plugin, eventName, v8Callback);
        }

        protected override void OnRemoveEventListener(CefV8Context context, string eventName, V8Callback v8Callback)
        {
            _router.RemoveEventListener(_plugin, eventName, v8Callback);
        }

        protected override bool OnGetProperty(string name, out object value)
        {
            if (!base.OnGetProperty(name, out value) && _plugin != null)
            {
                value = _plugin.GetProperty(name);
                return true;
            }
            return false;
        }

        protected override bool OnSetProperty(string name, CefV8Value value)
        {
            if (_plugin != null && _plugin.SetProperty(name, new CefJavaScriptValue(value)))
            {
                return true;
            }
            return base.OnSetProperty(name, value);
        }

        protected override void OnInvokeFunction(
            CefV8Context context,
            MethodDescriptor methodDescriptor,
            CefV8Value[] parameters,
            V8Callback v8Callback, out CefV8Value returnValue, out string exception)
        {
            returnValue = null;
            exception = null;
            LocalV8Callback localCallback = null;

            // If this call is executing without a callback, create a local callback that gathers the results
            if (v8Callback == null)
            {
                localCallback = new LocalV8Callback(this);
                v8Callback = localCallback;
            }
            
            _router.InvokeFunction(
                    context,
                    _plugin,
                    methodDescriptor,
                    new CefJavaScriptParameters(parameters),
                    v8Callback);
            
            // If we are executing with a local callback set the returnValue and exception from the values gathered by it.
            if (localCallback != null)
            {
                returnValue = localCallback.Result;
                exception = localCallback.Error;
            }
            
            exception = null;            
        }

        class LocalV8Callback : V8Callback
        {
            public CefV8Value Result{ get; private set; }
            public string Error { get; private set; }

            public LocalV8Callback(IV8Plugin plugin)
                : base(plugin, null, V8CallbackType.FunctionCallback, null)
            {
            }

            public override void Invoke(IV8PluginRouter router, CefV8Context context, object result, int errorCode, string error)
            {
                // No need to enter context, since we are executing synchronously
                try
                {
                    Result = this.ToCefV8Value(router, result);
                    Error = error;
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            }
        }
    }
}