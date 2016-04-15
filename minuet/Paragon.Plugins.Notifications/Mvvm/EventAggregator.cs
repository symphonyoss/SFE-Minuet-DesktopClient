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
using System.Collections.Generic;
using System.Threading;

namespace Paragon.Plugins.Notifications.Mvvm
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, EventBase> events = new Dictionary<Type, EventBase>();
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        public T GetEvent<T>() where T : EventBase, new()
        {
            lock (events)
            {
                EventBase existing;

                if (!events.TryGetValue(typeof (T), out existing))
                {
                    var newEvent = new T();
                    newEvent.SynchronizationContext = synchronizationContext;

                    events.Add(typeof (T), newEvent);

                    return newEvent;
                }

                return (T) existing;
            }
        }
    }
}