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
using System.Diagnostics;
using System.Security.Permissions;
using System.Windows.Forms;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WinForms
{
    /// <summary>
    /// This class provides a short-term fix for a CEF bug that causes the drop downs to appear at the wrong locations.
    /// </summary>
    internal class ContainerWindowMoveListener : NativeWindow, IDisposable
    {
        private readonly Action _onMove;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public ContainerWindowMoveListener(IntPtr handle, Action onMove)
        {
            _onMove = onMove;
            var h = Win32Api.GetToplevelParent(handle);
            if (h != IntPtr.Zero)
            {
                AssignHandle(h);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                ReleaseHandle();
                GC.SuppressFinalize(this);
            }
        }

        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int) WM.MOVE ||
                m.Msg == (int) WM.MOVING)
            {
                if (_onMove != null)
                {
                    _onMove();
                }
            }
            base.WndProc(ref m);
        }
    }
}