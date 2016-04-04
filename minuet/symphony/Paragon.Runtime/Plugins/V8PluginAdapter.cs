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

using System.Collections.Generic;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Adapts a native plugin to the V8 object model.
    /// </summary>
    public class V8PluginAdapter : CefUserData
    {
        private static readonly Dictionary<string, V8PluginAdapter> RemotePluginAdapterCache = 
            new Dictionary<string, V8PluginAdapter>();

        /// <summary>
        /// Create a new V8 plugin adapter.
        /// </summary>
        /// <param name="pluginObject">
        /// The local or remote native plugin to adapt to V8.
        /// </param>
        /// <param name="v8HostObject">
        /// Optional - if supplied, the adapter will add the native methods and events to this 
        /// existing object instead of creating a new one with a property accessor.
        /// </param>
        private V8PluginAdapter(IV8Plugin pluginObject, CefV8Value v8HostObject = null)
        {
            Plugin = pluginObject;
            V8Object = v8HostObject;
            if (V8Object == null)
            {
                V8Object = CefV8Value.CreateObject(new PluginV8Accessor());
                V8Object.SetUserData(this);
            }

            // Create a single method handler configured to handle all methods on the plugin
            var methodHandler = new PluginMethodV8Handler(Plugin);
            foreach (var methodDescriptor in Plugin.Descriptor.Methods)
            {
                V8Object.SetValue(methodDescriptor.MethodName,
                    CefV8Value.CreateFunction(methodDescriptor.MethodName, methodHandler),
                    CefV8PropertyAttribute.None);
            }

            foreach (var eventName in Plugin.Descriptor.Events)
            {
                // For each event, create a child object exposed as a property named after the event
                // The child object is a syntactic placeholder for the addListener/removeListener/hasListener/hasListeners methods

                var eventObject = CefV8Value.CreateObject(null);
                var eventHandler = new PluginEventV8Handler(Plugin, eventName);

                eventObject.SetValue(PluginEventV8Handler.MethodNameAddListener,
                    CefV8Value.CreateFunction(PluginEventV8Handler.MethodNameAddListener, eventHandler),
                    CefV8PropertyAttribute.None);
                eventObject.SetValue(PluginEventV8Handler.MethodNameRemoveListener,
                    CefV8Value.CreateFunction(PluginEventV8Handler.MethodNameRemoveListener, eventHandler),
                    CefV8PropertyAttribute.None);
                eventObject.SetValue(PluginEventV8Handler.MethodNameHasListener,
                    CefV8Value.CreateFunction(PluginEventV8Handler.MethodNameHasListener, eventHandler),
                    CefV8PropertyAttribute.None);
                eventObject.SetValue(PluginEventV8Handler.MethodNameHasListeners,
                    CefV8Value.CreateFunction(PluginEventV8Handler.MethodNameHasListeners, eventHandler),
                    CefV8PropertyAttribute.None);

                V8Object.SetValue(eventName, eventObject, CefV8PropertyAttribute.None);
            }
        }

        /// <summary>
        /// The native plugin this adapter connects to V8.
        /// </summary>
        public IV8Plugin Plugin { get; private set; }

        /// <summary>
        /// The V8 object that is populated with the methods and events from the native plugin.
        /// </summary>
        public CefV8Value V8Object { get; private set; }

        public static V8PluginAdapter FromCefObject(CefV8Value value)
        {
            if (value == null || !value.IsObject)
            {
                return null;
            }

            return value.GetUserData() as V8PluginAdapter;
        }

        public static V8PluginAdapter Create(IV8Plugin pluginObject, CefV8Value v8HostObject)
        {
            return new V8PluginAdapter(pluginObject, v8HostObject);
        }

        /// <summary>
        /// Create an adapter for a local (in-process) plugin.
        /// </summary>
        /// <param name="router"></param>
        /// <param name="pluginContext"></param>
        /// <param name="jsPlugin"></param>
        /// <returns></returns>
        public static V8PluginAdapter CreateLocal(IV8PluginRouter router, IPluginContext pluginContext, JavaScriptPlugin jsPlugin)
        {
            if (jsPlugin == null)
            {
                return null;
            }

            var plugin = new LocalV8Plugin(router, pluginContext, jsPlugin);
            var pluginAdapter = new V8PluginAdapter(plugin);
            return pluginAdapter;
        }

        /// <summary>
        /// Create an adapter for a remote (out-of-process) plugin.
        /// </summary>
        /// <param name="router"></param>
        /// <param name="pluginContext"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static V8PluginAdapter CreateRemote(IV8PluginRouter router, IPluginContext pluginContext, PluginDescriptor descriptor)
        {
            V8PluginAdapter adapter;
            if (!RemotePluginAdapterCache.TryGetValue(descriptor.PluginId, out adapter))
            {
                lock (RemotePluginAdapterCache)
                {
                    if (!RemotePluginAdapterCache.TryGetValue(descriptor.PluginId, out adapter))
                    {
                        var plugin = new RemoteV8Plugin(router, pluginContext, descriptor);
                        adapter = new V8PluginAdapter(plugin);
                        RemotePluginAdapterCache.Add(descriptor.PluginId, adapter);
                    }
                }
            }

            return adapter;
        }
    }
}