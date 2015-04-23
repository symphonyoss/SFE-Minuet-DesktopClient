using System;
using System.Collections.Generic;
using System.Threading;

namespace Symphony.Mvvm
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, EventBase> events = new Dictionary<Type, EventBase>();
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        public T GetEvent<T>() where T : EventBase, new()
        {
            lock (this.events)
            {
                EventBase existing;

                if (!this.events.TryGetValue(typeof (T), out existing))
                {
                    var newEvent = new T();
                    newEvent.SynchronizationContext = this.synchronizationContext;

                    this.events.Add(typeof (T), newEvent);

                    return newEvent;
                }
                
                return (T) existing;
            }
        }
    }
}
