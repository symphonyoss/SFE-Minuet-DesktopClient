using System;

namespace Paragon.Runtime.WPF
{
    internal class Lazy<T>
    {
        private static readonly object _lock = new object();
        private readonly Func<T> _resolver;
        private Boxed<T> _value;

        public Lazy(Func<T> resolver)
        {
            _resolver = resolver;
            _value = null;
        }

        public T Value
        {
            get
            {
                lock (_lock)
                {
                    if (_value == null && _resolver != null)
                    {
                        _value = new Boxed<T> {Value = _resolver()};
                    }
                    return _value != null ? _value.Value : default(T);
                }
            }
        }

        private class Boxed<TVal>
        {
            public TVal Value { get; set; }
        }
    }
}