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

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WinForms
{
    internal class NativeApplicationWindow : NativeWindow
    {
        private readonly List<HwndSourceHook> _hooks = new List<HwndSourceHook>();

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public NativeApplicationWindow(Window applicationWindow)
        {
            if (!applicationWindow.IsInitialized)
            {
                applicationWindow.SourceInitialized += (sender, args) => AssignHandle(sender as Window);
            }
            else
            {
                AssignHandle(applicationWindow);
            }
        }

        public void AddHook(HwndSourceHook hwndSourceHook)
        {
            _hooks.Add(hwndSourceHook);
        }

        #region Temporaty fix for bug in minimize/restore after Aero Snapping 

        private const int WIN_MINIMIZE_X = -32000;
        RECT _bounds;
        bool _boundsInitialized = false;
        private WindowState _state = WindowState.Normal;
        private WindowState _prevState = WindowState.Normal;

        private void OnWindowPosChanging(ref Message m)
        {
            var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
            bool changed = false;
            
            if ((pos.flags & (int)SWP.NOMOVE) != (int)SWP.NOMOVE)
            {
                if (!_boundsInitialized)
                {
                    _bounds = new RECT() { Left = pos.x, Top = pos.y, Right = pos.x + pos.cx, Bottom = pos.y + pos.cy };
                    _boundsInitialized = true;
                }
                // Correct messages with wrong location.
                if (_state == WindowState.Minimized && _prevState != WindowState.Maximized)
                {
                    if (pos.x != _bounds.Left)
                    {
                        pos.x = _bounds.Left;
                    }
                    if (pos.y != _bounds.Top)
                    {
                        pos.y = _bounds.Top;
                    }
                    changed = true;
                }
            }
            if ((pos.flags & (int)SWP.NOSIZE) != (int)SWP.NOSIZE)
            {
                if (!_boundsInitialized)
                {
                    _bounds = new RECT() { Left = pos.x, Top = pos.y, Right = pos.x + pos.cx, Bottom = pos.y + pos.cy };
                    _boundsInitialized = true;
                }
                // Correct messages with wrong size
                if (_state == WindowState.Minimized && _prevState != WindowState.Maximized)
                {
                    if (pos.cx != _bounds.Width)
                    {
                        pos.cx = _bounds.Width;
                    }
                    if (pos.cy != _bounds.Height)
                    {
                        pos.cy = _bounds.Height;
                    }
                    changed = true;
                }
            }
            if (!_boundsInitialized)
            {
                _bounds = RECT.FromHandle(Handle);
                _boundsInitialized = true;
            }
            if (changed)
            {
                Marshal.StructureToPtr(pos, m.LParam, true);
            }
        }

        private void OnWindowPosChanged(ref Message m)
        {
            var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
            if ((pos.flags & (int)SWP.NOMOVE) != (int)SWP.NOMOVE && pos.x != WIN_MINIMIZE_X)
            {
                _bounds.Right = _bounds.Width + pos.x;
                _bounds.Bottom = _bounds.Height + pos.y;
                _bounds.Left = pos.x;
                _bounds.Top = pos.y;
            }
            if ((pos.flags & (int)SWP.NOSIZE) != (int)SWP.NOSIZE && pos.x != WIN_MINIMIZE_X)
            {
                _bounds.Right = pos.x + pos.cx;
                _bounds.Bottom = pos.y + pos.cy;
            }
        }

        private void OnSizeChanged(ref Message m)
        {
            switch ((int)m.WParam)
            {
                case (int)SizeChangeType.RESTORED:
                    _prevState = _state;
                    _state = WindowState.Normal;
                    break;

                case (int)SizeChangeType.MINIMIZED:
                    _prevState = _state;
                    _state = WindowState.Minimized;
                    break;

                case (int)SizeChangeType.MAXIMIZED:
                    _prevState = _state;
                    _state = WindowState.Maximized;
                    break;
            }
        }
        #endregion

        [DebuggerStepThrough]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            // The following code fixes the improper restoration from minimized state after Aero Snapping (Win+Left/Right)
            switch (m.Msg)
            {
                case (int)WM.WINDOWPOSCHANGING:
                    OnWindowPosChanging(ref m);
                    break;
                case (int)WM.WINDOWPOSCHANGED:
                    OnWindowPosChanged(ref m);
                    break;
                case (int) WM.SIZE:
                    OnSizeChanged(ref m);
                    break;
            }

            foreach (var hook in _hooks.ToArray())
            {
                var handled = false;

                m.Result = hook.Invoke(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);

                if (handled)
                {
                    return;
                }
            }

            base.WndProc(ref m);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void AssignHandle(Window window)
        {
            AssignHandle(new WindowInteropHelper(window).Handle);
        }
    }
}