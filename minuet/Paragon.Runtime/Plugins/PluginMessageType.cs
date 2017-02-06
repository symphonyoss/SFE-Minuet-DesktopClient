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

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines the types of <see cref="PluginMessage"/> that can be sent and received.
    /// </summary>
    [Flags]
    public enum PluginMessageType
    {
        Invalid = 0,

        /// <summary>
        /// Configure plugins for a browser instance.
        /// </summary>
        InitializePlugins = 1,

        /// <summary>
        /// The invocation of a function exposed by a plugin.
        /// </summary>
        FunctionInvoke = 2,

        /// <summary>
        /// The response to a function invocation.
        /// </summary>
        FunctionCallback = 4,

        /// <summary>
        /// A callback passed into a function invocation rather than 
        /// being invoked by the framework when the function completes.
        /// </summary>
        ParameterCallback = FunctionCallback | Retained,

        /// <summary>
        /// The addition of a remote callback to a plugin event.
        /// </summary>
        AddListener = 32 | Retained,

        /// <summary>
        /// Sent to registered listener callbacks when an event is fired.
        /// </summary>
        EventFired = 64,

        /// <summary>
        /// Indicates the message is a callback or listener that needs to be 
        /// retained until it is explicitly unsubscribed or the context is released.
        /// </summary>
        Retained = 128,

        /// <summary>
        /// Remove a retained callback or listener
        /// </summary>
        RemoveRetained = 256,

        /// <summary>
        /// Kill the Renderer process so we can start cleanly
        /// </summary>
        KillRenderer = 512,
    }
}