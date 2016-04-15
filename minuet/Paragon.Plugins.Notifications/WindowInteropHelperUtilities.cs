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
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

namespace Paragon.Plugins.Notifications
{
    public static class WindowInteropHelperUtilities
    {
        public static IntPtr EnsureHandle(this WindowInteropHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException("helper");
            }

            if (helper.Handle == IntPtr.Zero)
            {
                var window = (Window) typeof (WindowInteropHelper).InvokeMember(
                    "_window",
                    BindingFlags.GetField |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic,
                    null, helper, null);

                try
                {
                    // SafeCreateWindow only exists in the .NET 2.0 runtime. If we try to
                    // invoke this method on the .NET 4.0 runtime it will result in a
                    // MissingMethodException, see below.
                    typeof(Window).InvokeMember(
                        "SafeCreateWindow",
                        BindingFlags.InvokeMethod |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic,
                        null, window, null);
                }
                catch (MissingMethodException)
                {
                    // If we ended up here it means we are running on the .NET 4.0 runtime,
                    // where the method we need to call for the handle was renamed/replaced
                    // with CreateSourceWindow.
                    typeof(Window).InvokeMember(
                        "CreateSourceWindow",
                        BindingFlags.InvokeMethod |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic,
                        null, window, new object[] { false });
                }
            }

            return helper.Handle;
        }
    }
}