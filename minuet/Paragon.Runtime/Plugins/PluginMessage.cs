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
    /// Defines a common format for interacting with a plugin across the browser | shared plugin | render | application plugin processes.
    /// </summary>
    public class PluginMessage
    {
        /// <summary>
        /// Provides correlation of interactions across the plugin boundaries.
        /// </summary>
        /// <remarks>
        /// This value depends on the value of <see cref="MessageType"/>:
        /// 
        /// For <see cref="PluginMessageType.FunctionInvoke"/> and <see cref="PluginMessageType.AddListener"/> this is a new Guid.
        /// For <see cref="PluginMessageType.FunctionCallback"/> it is the value from the corresponding <see cref="PluginMessageType.FunctionInvoke"/>.
        /// For <see cref="PluginMessageType.RemoveRetained"/> and <see cref="PluginMessageType.EventFired"/> is the value from the corresponding <see cref="PluginMessageType.AddListener"/>.
        /// </remarks>
        public Guid MessageId { get; set; }

        /// <summary>
        /// The type of interaction the message defines.
        /// </summary>
        public PluginMessageType MessageType { get; set; }

        /// <summary>
        /// The identifier of the plugin the message concerns.
        /// </summary>
        public string PluginId { get; set; }

        /// <summary>
        /// The name of the function or event the message is intended for.
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// The JSON encoded payload for the receiver of the message.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// The CefBrowser Identifier if the message relates to a call from V8.
        /// </summary>
        public int BrowserId { get; set; }

        /// <summary>
        /// The identifier for the V8 context if the message relates to a call from V8.
        /// </summary>
        public int ContextId { get; set; }

        /// <summary>
        /// The identifier of the V8 frame if the message relates to a call from V8.
        /// </summary>
        public long FrameId { get; set; }

        /// <summary>
        /// Only valid for <see cref="PluginMessageType.FunctionInvoke"/> and <see cref="PluginMessageType.ParameterCallback"/>,
        /// if not <see cref="Guid.Empty"/>, specifies the indentifier of a callback supplied by the invoking JavaScript code.
        /// </summary>
        public Guid V8CallbackId { get; set; }
    }
}