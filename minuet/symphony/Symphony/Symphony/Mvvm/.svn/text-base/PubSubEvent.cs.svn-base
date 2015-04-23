using System;
using System.Collections.Generic;
using System.Linq;

namespace Symphony.Mvvm
{
    public abstract class PubSubEvent<TArgs> : EventBase
        where TArgs : class
    {
        private readonly List<Subscription> subscriptions = new List<Subscription>();

        public virtual void Publish(TArgs args)
        {
            lock (this)
            {
                var toExecute = this.subscriptions
                    .ToList();

                toExecute.ForEach(subscription =>
                    {
                        if (subscription.Options == SubscriptionOptions.Dispatcher)
                        {
                            this.SynchronizationContext.Post(state => subscription.Action((TArgs)state), args);
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
            this.Subscribe(action, SubscriptionOptions.Dispatcher);
        }

        public virtual void Subscribe(Action<TArgs> action, SubscriptionOptions options)
        {
            lock (this)
            {
                this.subscriptions.Add(
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
                var sub = this.subscriptions.FirstOrDefault(s => s.Action == action);
                if (sub != null) this.subscriptions.Remove(sub);
            }
        }

        private class Subscription
        {
            public Action<TArgs> Action { get; set; }
            public SubscriptionOptions Options { get; set; }
        }
    }
}
