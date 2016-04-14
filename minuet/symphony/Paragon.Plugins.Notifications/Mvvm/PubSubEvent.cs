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
using System.Linq;

namespace Paragon.Plugins.Notifications.Mvvm
{
    public abstract class PubSubEvent<TArgs> : EventBase
        where TArgs : class
    {
        private readonly List<Subscription> subscriptions = new List<Subscription>();

        public virtual void Publish(TArgs args)
        {
            lock (this)
            {
                var toExecute = subscriptions
                    .ToList();

                toExecute.ForEach(subscription =>
                {
                    if (subscription.Options == SubscriptionOptions.Dispatcher)
                    {
                        SynchronizationContext.Post(state => subscription.Action((TArgs) state), args);
                    }
                    else
                    {
                        subscription.Action(args);
                    }
                });
            }
        }

        public virtual void Subscribe(Action<TArgs> action)
        {
            Subscribe(action, SubscriptionOptions.Dispatcher);
        }

        public virtual void Subscribe(Action<TArgs> action, SubscriptionOptions options)
        {
            lock (this)
            {
                subscriptions.Add(
                    new Subscription
                    {
                        Action = action,
                        Options = options
                    });
            }
        }

        public virtual void Unsubscribe(Action<TArgs> action)
        {
            lock (this)
            {
                var sub = subscriptions.FirstOrDefault(s => s.Action == action);
                if (sub != null)
                {
                    subscriptions.Remove(sub);
                }
            }
        }

        private class Subscription
        {
            public Action<TArgs> Action { get; set; }
            public SubscriptionOptions Options { get; set; }
        }
    }
}