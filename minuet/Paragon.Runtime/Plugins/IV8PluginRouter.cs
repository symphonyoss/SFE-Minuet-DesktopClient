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

using Newtonsoft.Json.Linq;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public interface IV8PluginRouter
    {
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
        void InvokeFunction(CefV8Context context, PluginDescriptor targetPlugin, MethodDescriptor methodDescriptor, JArray parameters, IV8Callback callback);

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
        /// The callback to V8 to invoke when the event is raised.
        /// </param>
        void AddEventListener(CefV8Context context, PluginDescriptor targetPlugin, string eventName, IV8Callback callback);

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
        void RemoveEventListener(CefV8Context context, PluginDescriptor targetPlugin, string eventName, IV8Callback callback);

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
        void InvokeFunction(CefV8Context context, JavaScriptPlugin targetPlugin, MethodDescriptor methodDescriptor, IJavaScriptParameters parameters, IV8Callback callback);

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
        void AddEventListener(CefV8Context context, JavaScriptPlugin targetPlugin, string eventName, IV8Callback callback);

        void RemoveEventListener(JavaScriptPlugin targetPlugin, string eventName, IV8Callback callback);
    }
}