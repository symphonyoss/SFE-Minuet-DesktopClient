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