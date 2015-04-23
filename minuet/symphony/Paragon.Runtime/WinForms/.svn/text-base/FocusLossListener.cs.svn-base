using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Windows.Forms;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WinForms
{
    internal class FocusLossListener : NativeWindow, IDisposable
    {
        private readonly List<WidgetWindow> _widgetWindows = new List<WidgetWindow>();
        private bool _topMost;
        private WidgetWindowCreationListener _wwListener;

        public FocusLossListener(IntPtr handle)
        {
            var h = Win32Api.GetChildWindow(handle);
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
                if (_wwListener != null)
                {
                    _wwListener.Dispose();
                }
                ReleaseHandle();
                GC.SuppressFinalize(this);
            }
        }

        public void SetTopMost(bool set)
        {
            _topMost = set;
        }

        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int) WM.PARENTNOTIFY:
                    if (_topMost)
                    {
                        if ((((uint) m.WParam) & (uint) WM.LBUTTONDOWN) == (uint) WM.LBUTTONDOWN)
                        {
                            if (_wwListener != null)
                            {
                                _wwListener.Dispose();
                                _wwListener = null;
                            }
                            _wwListener = new WidgetWindowCreationListener(OnWidgetWindowCreated);
                        }
                        else if ((((uint) m.WParam) & (uint) WM.LBUTTONUP) == (uint) WM.LBUTTONUP)
                        {
                            if (_wwListener != null)
                            {
                                _wwListener.Dispose();
                                _wwListener = null;
                            }
                        }
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void OnWidgetWindowCreated(IntPtr hWnd, string className)
        {
            if (_topMost)
            {
                if (className.StartsWith("Chrome_Widget", StringComparison.InvariantCultureIgnoreCase))
                {
                    _widgetWindows.Add(new WidgetWindow(hWnd, OnWidgetWindowDestroyed));
                }
            }
        }

        private void OnWidgetWindowDestroyed(WidgetWindow w)
        {
            if (_widgetWindows.Contains(w))
            {
                _widgetWindows.Remove(w);
            }
        }
    }
}