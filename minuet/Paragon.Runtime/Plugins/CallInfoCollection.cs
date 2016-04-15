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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Paragon.Plugins;

namespace Paragon.Runtime.Plugins
{
    public sealed class CallInfoCollection<TCallInfo> : IDisposable where TCallInfo : CallInfo
    {
        private readonly ILogger _logger = ParagonLogManager.GetLogger();

        private readonly ConcurrentDictionary<Guid, TCallInfo> _pendingCallbacks =
            new ConcurrentDictionary<Guid, TCallInfo>();

        private bool _disposed;

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

            var callbacks = _pendingCallbacks.Values.ToList();
            _pendingCallbacks.Clear();
            callbacks.ForEach(c => c.Dispose());
        }

        public void Add(TCallInfo info)
        {
            var pluginMessage = info.RequestMessage;
            if (!_pendingCallbacks.TryAdd(pluginMessage.MessageId, info))
            {
                _logger.Error("Duplicate callback encountered");
            }
        }

        public void Remove(TCallInfo info)
        {
            var pluginMessage = info.RequestMessage;
            Remove(pluginMessage.MessageId);
        }

        public TCallInfo Remove(Guid callId)
        {
            TCallInfo callback;
            if (!_pendingCallbacks.TryRemove(callId, out callback))
            {
                _logger.Warn("Callback not found");
            }

            return callback;
        }

        public List<TCallInfo> RemoveWhere(Func<TCallInfo, bool> removePredicate)
        {
            var removedCalls = new List<TCallInfo>();

            var callsToRemove = _pendingCallbacks.Values
                .Where(call => removePredicate == null || removePredicate(call)).ToList();

            foreach (var call in callsToRemove)
            {
                TCallInfo removedCall;
                if (_pendingCallbacks.TryRemove(call.RequestMessage.MessageId, out removedCall))
                {
                    removedCalls.Add(removedCall);
                }
            }

            return removedCalls;
        }

        public TCallInfo Get(Guid callId)
        {
            TCallInfo call;
            if (!_pendingCallbacks.TryGetValue(callId, out call))
            {
                _logger.Warn("Callback not found");
            }

            return call;
        }
    }
}