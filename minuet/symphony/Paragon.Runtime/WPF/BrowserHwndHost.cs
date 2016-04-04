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
using Paragon.Runtime.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Paragon.Runtime.WPF
{
    /// <summary>
    /// This class is the base class for the CefWebBrowser control.
    /// </summary>
    public abstract class BrowserHwndHost : HwndHost
    {
        public static IntPtr HWND_MESSAGE = new IntPtr(-3);
        private IntPtr _hWndParent = IntPtr.Zero;
        public event EventHandler HandleCreated;

        /// <summary>
        /// The window handle of the browser
        /// </summary>
        public abstract IntPtr BrowserWindowHandle { get; }

        public IntPtr ParentHandle
        {
            get
            {
                return _hWndParent != IntPtr.Zero ? _hWndParent : HWND_MESSAGE;
            }
        }

        /// <summary>
        /// Noop. Kept for backward compatibility.
        /// </summary>
        public virtual void CreateControl()
        {
        }

        /// <summary>
        /// This method re-parents the browser window to the permanent parent window.
        /// </summary>
        /// <param name="hWnd">permanent parent window</param>
        /// <returns>browser window handle ref</returns>
        protected override HandleRef BuildWindowCore(HandleRef hWnd)
        {
            _hWndParent = hWnd.Handle;
            if (BrowserWindowHandle != IntPtr.Zero)
            {
                ReparentControl();
                return new HandleRef(this, BrowserWindowHandle);
            }
            return new HandleRef(this, HWND_MESSAGE);
        }

        /// <summary>
        /// This method re-parents the browser to the current parent window
        /// </summary>
        private void ReparentControl()
        {
            if (_hWndParent != IntPtr.Zero && BrowserWindowHandle != IntPtr.Zero)
            {
                RECT rect = new RECT();
                Win32Api.GetClientRect(_hWndParent, ref rect);
                Win32Api.SetParent(BrowserWindowHandle, _hWndParent);
                ResizeWindow(rect.Width, rect.Height);
            }
        }

        /// <summary>
        /// Noop. The child window is destroyed by CEF.
        /// </summary>
        /// <param name="hwnd"></param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }

        protected virtual void OnHandleCreated()
        {
            ReparentControl();
            if (HandleCreated != null)
                HandleCreated(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hWndParent = IntPtr.Zero;
            }
            base.Dispose(disposing);
        }

        protected void ResizeWindow(int width, int height)
        {
            // Ignore size changes when form are minimized.
            if( BrowserWindowHandle != IntPtr.Zero)
                Win32Api.SetWindowPosition(BrowserWindowHandle, IntPtr.Zero, 0, 0, width, height, SWP.NOZORDER | SWP.NOACTIVATE);
        }

        protected void DispatchIfRequired(Action a, bool isAsync = false)
        {
            if (!Dispatcher.CheckAccess())
            {
                if (isAsync)
                {
                    Dispatcher.BeginInvoke(a);
                }
                else
                {
                    Dispatcher.Invoke(a);
                }
            }
            else
            {
                a();
            }
        }
    }
}
