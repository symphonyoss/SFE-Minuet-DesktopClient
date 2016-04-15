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
    public abstract class CallInfo : IDisposable
    {
        public const int ErrorCodeCallCanceled = -1;
        public const string ErrorCallCanceled = "The call has been canceled";
        public const string Call = "GS_Call";
        private bool _disposed;

        protected CallInfo(PluginMessage requestMessage, string pluginId = null)
        {
            RequestMessage = requestMessage;
            PluginId = pluginId;
        }

        public PluginMessage RequestMessage { get; private set; }

        public string PluginId { get; private set; }

        public int ContextId
        {
            get { return RequestMessage.ContextId; }
        }

        public bool IsEventListener
        {
            get { return RequestMessage.MessageType == PluginMessageType.AddListener; }
        }

        public bool IsRetained
        {
            get { return (RequestMessage.MessageType & PluginMessageType.Retained) == PluginMessageType.Retained; }
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}