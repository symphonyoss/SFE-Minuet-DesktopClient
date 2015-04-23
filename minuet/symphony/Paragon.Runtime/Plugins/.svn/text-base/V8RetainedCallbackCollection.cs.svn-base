using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public class V8RetainedCallbackCollection
    {
        private readonly ConcurrentDictionary<string, List<V8Callback>> _callbacksByMemberName =
            new ConcurrentDictionary<string, List<V8Callback>>();

        public void Clear()
        {
            var callbacks = _callbacksByMemberName.Values;
            _callbacksByMemberName.Clear();
            foreach (var list in callbacks)
            {
                list.ForEach(c => c.Dispose());
                list.Clear();
            }
        }

        public V8Callback GetOrCreateCallback(IV8Plugin plugin, string memberName, CefV8Value callback, V8CallbackType callbackType)
        {
            V8Callback adapter = null;

            _callbacksByMemberName.AddOrUpdate(
                memberName,
                key =>
                {
                    var disposeCallback = new Action<V8Callback>(cb => DisposeCallback(memberName, cb));
                    adapter = new V8Callback(plugin, callback, callbackType, disposeCallback);
                    return new List<V8Callback> {adapter};
                },
                (key, list) =>
                {
                    adapter = list.FirstOrDefault(c => c.CallbackFunction != null && c.CallbackFunction.IsSame(callback));
                    if (adapter == null)
                    {
                        var disposeCallback = new Action<V8Callback>(cb => DisposeCallback(memberName, cb));
                        adapter = new V8Callback(plugin, callback, callbackType, disposeCallback);
                        list.Add(adapter);
                    }

                    return list;
                });

            return adapter;
        }

        public V8Callback GetAndRemoveCallback(string memberName, CefV8Value callback)
        {
            List<V8Callback> callbacks;
            if (!_callbacksByMemberName.TryGetValue(memberName, out callbacks))
            {
                return null;
            }

            var callbackAdapter = callbacks.FirstOrDefault(c => c.CallbackFunction.IsSame(callback));
            if (callbackAdapter == null)
            {
                return null;
            }

            callbacks.Remove(callbackAdapter);
            return callbackAdapter;
        }

        public bool HasCallback(string memberName, CefV8Value callback)
        {
            List<V8Callback> callbacks;
            return _callbacksByMemberName.TryGetValue(memberName, out callbacks)
                   && callbacks.Any(c => c.CallbackFunction.IsSame(callback));
        }

        public bool HasCallbacks(string memberName)
        {
            List<V8Callback> callbacks;
            return _callbacksByMemberName.TryGetValue(memberName, out callbacks)
                   && (callbacks != null && callbacks.Count != 0);
        }

        private void DisposeCallback(string memberName, V8Callback callback)
        {
            List<V8Callback> callbacks;
            if (_callbacksByMemberName.TryGetValue(memberName, out callbacks))
            {
                callbacks.Remove(callback);
            }
        }
    }
}