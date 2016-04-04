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
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Maps a plugin to CEF/V8.
    /// </summary>
    public interface IV8Plugin : IDisposable
    {
        /// <summary>
        /// Returns a descriptor for the plugin.
        /// </summary>
        PluginDescriptor Descriptor { get; }

        IPluginContext PluginContext { get; }

        /// <summary>
        /// Add a JavaScript listener function to a native event.
        /// </summary>
        /// <param name="context">The current V8 JavaScript context.</param>
        /// <param name="eventName">The name of the event to attach the function to.</param>
        /// <param name="callback"> The function to attach to the event.</param>
        void AddEventListener(CefV8Context context, string eventName, CefV8Value callback);

        /// <summary>
        /// Remove a JavaScript listener function from a native event.
        /// </summary>
        /// <param name="context">The current V8 JavaScript context.</param>
        /// <param name="eventName">The name of the event to detach the function from.</param>
        /// <param name="callback"> The function to detach from the event.</param>
        void RemoveEventListener(CefV8Context context, string eventName, CefV8Value callback);

        /// <summary>
        /// Determines if the plugin already has a particular JavaScript function added as a listener.
        /// </summary>
        /// <param name="eventName">The name of the event to check.</param>
        /// <param name="callback">The function to check has already been added.</param>
        /// <returns>True if the function was already added; false otherwise.</returns>
        bool HasEventListener(string eventName, CefV8Value callback);

        /// <summary>
        /// Determines if the plugin has any JavaScript listener functions added for a particular event.
        /// </summary>
        /// <param name="eventName">The name of the event to check.</param>
        /// <returns>True if the event has any JavaScript listener functions; false otherwise.</returns>
        bool HasEventListeners(string eventName);

        /// <summary>
        /// Get the value of a named property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="returnValue"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool GetProperty(string name, out CefV8Value returnValue, out string exception);

        /// <summary>
        /// Set the value of a named property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool SetProperty(string name, CefV8Value value, out string exception);

        /// <summary>
        /// Execute a native function.
        /// </summary>
        /// <param name="context">The current V8 JavaScript context.</param>
        /// <param name="functionName">The name of the native function to invoke.</param>
        /// <param name="parameters">The parameters to pass to the function.</param>
        /// <param name="callback">An optional callback for the native function to invoke when it is complete.</param>
        void ExecuteFunction(CefV8Context context, string functionName, CefV8Value[] parameters, CefV8Value callback, out CefV8Value returnValue, out string exception);
    }
}