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
using System.Linq;
using System.Security.Permissions;
using System.Windows;
using Paragon.Runtime.Win32;
using Paragon.Runtime.WinForms;

namespace Paragon.Runtime.Kernel.Windowing
{
    internal class SystemMenuInterceptor
    {
        private readonly SystemMenuOptions _options;
        private IntPtr _systemMenu = IntPtr.Zero;

        public SystemMenuInterceptor(SystemMenuOptions options)
        {
            _options = options;
        }

        public event EventHandler<ItemClickedEventArgs> ItemClicked;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void ApplyTo(Window applicationWindow)
        {
            var nativeWindow = new NativeApplicationWindow(applicationWindow);
            _systemMenu = NativeMethods.GetSystemMenu(nativeWindow.Handle, false);
            if (_options.Items.Any())
            {
                NativeMethods.InsertMenu(_systemMenu, -1, (uint) (MF.BYPOSITION | MF.SEPARATOR), UIntPtr.Zero, String.Empty);
            }
            foreach (var item in _options.Items)
            {
                var flags = MF.BYCOMMAND | MF.STRING;
                if (item.Checkable)
                {
                    flags |= MF.CHECKED;
                }
                NativeMethods.InsertMenu(_systemMenu, item.Id, (uint) flags, new UIntPtr((uint)item.Id), item.Header);
            }

            nativeWindow.AddHook(WndProc);
        }

        [DebuggerStepThrough]
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM) msg)
            {
                case WM.SYSCOMMAND:
                    var mitem = _options.Items.FirstOrDefault(item => item.Id == wParam.ToInt32());
                    if (mitem != null)
                    {
                        if (mitem.Checkable)
                        {
                            mitem.IsChecked = !mitem.IsChecked;
                        }
                        var handler = ItemClicked;
                        if (handler != null)
                        {
                            handler(this, new ItemClickedEventArgs(mitem.Id, mitem.IsChecked));
                            handled = true;
                        }
                    }
                    break;

                case WM.INITMENUPOPUP:
                    if (_systemMenu == wParam)
                    {
                        foreach (var item in _options.Items)
                        {
                            NativeMethods.EnableMenuItem(_systemMenu, (uint) item.Id,
                                (uint) (item.Enabled ? MF.ENABLED : MF.DISABLED));
                            if (item.Checkable)
                            {
                                NativeMethods.CheckMenuItem(_systemMenu, (uint) item.Id,
                                    (uint) (item.IsChecked ? MF.CHECKED : MF.UNCHECKED));
                            }
                        }

                        handled = true;
                    }

                    break;
            }

            return IntPtr.Zero;
        }
    }
}