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
using System.Threading;

namespace Paragon.Plugins
{
    public sealed class PluginExecutionContext : IDisposable
    {
        private const string TlsSlotName = "__ParagonPluginExecutionContext___";
        private static readonly LocalDataStoreSlot CtxSlot;

        private readonly int _browserId;
        private readonly long _frameId;
        private readonly int _javaScriptContextId;
        private bool _disposed;

        static PluginExecutionContext()
        {
            CtxSlot = Thread.AllocateNamedDataSlot(TlsSlotName);
        }

        private PluginExecutionContext(int browserId, int javaScriptContextId, long frameId)
        {
            _browserId = browserId;
            _javaScriptContextId = javaScriptContextId;
            _frameId = frameId;
        }

        private static PluginExecutionContext Current
        {
            get { return Thread.GetData(CtxSlot) as PluginExecutionContext; }
        }

        public static int BrowserIdentifier
        {
            get
            {
                var c = Current;
                return c != null ? c._browserId : -1;
            }
        }

        public static int JavaScriptContextIdentifier
        {
            get
            {
                var c = Current;
                return c != null ? c._javaScriptContextId : -1;
            }
        }

        public static long FrameIdentifier
        {
            get
            {
                var c = Current;
                return c != null ? c._frameId : -1;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Thread.SetData(CtxSlot, null);
            _disposed = true;
        }

        public static PluginExecutionContext Create(int browserId, int javaScriptContextId, long frameId)
        {
            var ctx = new PluginExecutionContext(browserId, javaScriptContextId, frameId);
            Thread.SetData(CtxSlot, ctx);
            return ctx;
        }
    }
}