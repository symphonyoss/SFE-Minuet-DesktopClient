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
using System.Linq;
using System.Reflection;
using Paragon.Plugins;

namespace Paragon.Runtime.Plugins
{
    public sealed class JavaScriptEvent : IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();

        private readonly ConcurrentDictionary<Guid, IJavaScriptPluginCallback> _callbacks =
            new ConcurrentDictionary<Guid, IJavaScriptPluginCallback>();

        private readonly EventInfo _eventInfo;
        private readonly JavaScriptPluginCallback _handler;
        private readonly object _nativeObject;

        private bool _disposed;

        public JavaScriptEvent(object nativeObject, EventInfo eventInfo)
        {
            ThrowIfDisposed();

            if (eventInfo == null)
            {
                throw new ArgumentNullException("eventInfo");
            }

            _nativeObject = nativeObject;
            _eventInfo = eventInfo;
            _handler = OnEventFired;
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

            _eventInfo.RemoveEventHandler(_nativeObject, _handler);
            _callbacks.Clear();
            _disposed = true;
        }

        public void AttachToEvent()
        {
            ThrowIfDisposed();
            _eventInfo.AddEventHandler(_nativeObject, _handler);
        }

        public void AddHandler(IJavaScriptPluginCallback callback)
        {
            ThrowIfDisposed();

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            _callbacks.TryAdd(callback.Identifier, callback);
        }

        public void RemoveHandler(IJavaScriptPluginCallback callback)
        {
            ThrowIfDisposed();

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            IJavaScriptPluginCallback removedCallback;
            _callbacks.TryRemove(callback.Identifier, out removedCallback);
        }

        private void OnEventFired(params object[] args)
        {
            var callbacks = _callbacks.Values.ToArray();

            try
            {
                foreach (var javaScriptPluginCallback in callbacks)
                {
                    javaScriptPluginCallback.Invoke(args, 0, string.Empty);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error invoking plugin callback: " + e);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Attempt to use a disposed instance of " + GetType().Name);
            }
        }
    }
}