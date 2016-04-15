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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Paragon.Runtime.Win32
{
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    internal class MessageWindow : IDisposable
    {
        private readonly List<WndProcDelegate> _hooks = new List<WndProcDelegate>();
        private bool _disposed;
        private WndProcDelegate _wndProcDelegate;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public MessageWindow(string className)
        {
            RegisterClass(className);

            Hwnd = NativeMethods.CreateWindowExW(0, className, "PargonApp", 0, 0, 0, 0, 0,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        public IntPtr Hwnd { get; private set; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (Hwnd != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(Hwnd);
                Hwnd = IntPtr.Zero;
            }

            _hooks.Clear();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void RegisterClass(string className)
        {
            _wndProcDelegate = WndProc;

            var wndClass = new WNDCLASS
            {
                lpszClassName = className,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate)
            };

            const int errorClassAlreadyExists = 1410;
            var atom = NativeMethods.RegisterClassW(ref wndClass);
            var lastError = Marshal.GetLastWin32Error();
            if (atom == 0 && lastError != errorClassAlreadyExists)
            {
                throw new InvalidOperationException("Window class registration failed");
            }
        }

        [DebuggerStepThrough]
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            return _hooks.ToArray().Any(hook => hook(hWnd, msg, wParam, lParam) == IntPtr.Zero)
                ? IntPtr.Zero : NativeMethods.DefWindowProcW(hWnd, msg, wParam, lParam);
        }
    }
}