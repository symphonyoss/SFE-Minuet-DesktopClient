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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace Paragon.Plugins.Notifications.Win32
{
    public static class Win32Api
    {
        /// <summary>
        /// Moves and shows a window at the top of the Z-order without activating it.
        /// </summary>
        /// <param name="handle">
        /// The HWND of the target window.
        /// </param>
        /// <param name="targetScreen">
        /// The screen to move and maximize the window to. If this is null, no size or move is performed, just the bring to top.
        /// </param>
        internal static void MoveAndShowWithNoActivate(IntPtr handle, IMonitor targetScreen)
        {
            // http://blogs.msdn.com/b/oldnewthing/archive/2005/11/21/495246.aspx
            // As a result of the introduction of "topmost" windows, HWND_TOP now brings the window "as high in the Z-order as 
            // possible without violating the rule that topmost windows always appear above non-topmost windows".
            // What does this mean in practice?
            // - If a window is topmost, then HWND_TOP puts it at the very top of the Z-order.
            // - If a window is not topmost, then HWND_TOP puts it at the top of all non-topmost windows (i.e., just below the lowest topmost window, if any).
            var flags = NativeMethods.SWP.NOACTIVATE | NativeMethods.SWP.SHOWWINDOW | NativeMethods.SWP.ASYNCWINDOWPOS;
            var bounds = targetScreen != null ? targetScreen.WorkingArea : Rect.Empty;
            if (bounds.IsEmpty)
            {
                flags |= NativeMethods.SWP.NOSIZE | NativeMethods.SWP.NOMOVE;
            }
            NativeMethods.SetWindowPos(
                handle,
                NativeMethods.HWND_TOP,
                (int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height,
                (uint)flags);
        }

        internal static void AddToolWindowStyle(IntPtr handle)
        {
            var exStyle = (uint)NativeMethods.GetWindowLong(handle, -20);
            exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
            SetWindowLong(handle, -20, (IntPtr)exStyle);
        }

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int) intPtr.ToInt64());
        }

        private static IntPtr SetWindowLong(IntPtr handle, int nIndex, IntPtr dwNewLong)
        {
            int error;

            var result = IntPtr.Zero;

            NativeMethods.SetLastError(0);

            if (IntPtr.Size == 4)
            {
                var tempResult = NativeMethods.SetWindowLong(handle, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                result = NativeMethods.SetWindowLongPtr(handle, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if (result == IntPtr.Zero && error != 0)
            {
                throw new Win32Exception(error);
            }

            return result;
        }
    }
}