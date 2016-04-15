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
using Paragon.Plugins;

namespace Paragon.Runtime
{
    public static class EventHandlerExtensions
    {
        public static void Raise<T>(this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            EventHandler<T> local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local(sender, args);
            }
        }

        public static void Raise(this EventHandler handler, object sender, EventArgs args)
        {
            EventHandler local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local(sender, args);
            }
        }

        public static void Raise(this JavaScriptPluginCallback handler)
        {
            JavaScriptPluginCallback local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local();
            }
        }

        public static void Raise(this JavaScriptPluginCallback handler, Func<object[]> getArgs)
        {
            JavaScriptPluginCallback local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local(getArgs());
            }
        }
    }
}