using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WinForms
{
    /// <summary>
    /// This class provides a short-term fix for a CEF bug that causes the dropdown windows (widget windows) get hidden
    /// behind the browser window, if the browser window is set to "always on top". This should go away soon
    /// </summary>
    internal class WidgetWindowZOrderHandler : NativeWindow, IDisposable
    {
        private readonly List<WidgetWindow> _widgetWindows = new List<WidgetWindow>();
        private bool _topMost;
        private WidgetWindowCreationListener _wwListener;

        public WidgetWindowZOrderHandler(IntPtr handle)
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

        private class WidgetWindow : NativeWindow, IDisposable
        {
            private static readonly ILogger Logger = ParagonLogManager.GetLogger();
            private readonly Action<WidgetWindow> _onDispose;

            public WidgetWindow(IntPtr hWnd, Action<WidgetWindow> onDispose)
            {
                AssignHandle(hWnd);
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                if (Handle != IntPtr.Zero)
                {
                    if (_onDispose != null)
                    {
                        _onDispose(this);
                    }
                    ReleaseHandle();
                    GC.SuppressFinalize(this);
                }
            }

            [DebuggerStepThrough]
            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case (int)WM.DESTROY:
                        Dispose();
                        break;

                    case (int)WM.WINDOWPOSCHANGING:
                        {
                            var style = Win32Api.GetStyle(Handle);
                            if ((style & (long)WS.POPUP) == (long)WS.POPUP)
                            {
                                Logger.Info(string.Format("Setting popup window {0} created on thread {1} to be top-most", m.HWnd, Win32Api.GetWindowThreadId(m.HWnd)));
                                var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
                                pos.hwndInsertAfter = new IntPtr(-1);
                                pos.flags = (uint)(pos.flags & ~0x0004);
                                Marshal.StructureToPtr(pos, m.LParam, true);
                            }
                        }
                        break;
                }

                base.WndProc(ref m);
            }
        }

        private class WidgetWindowCreationListener : IDisposable
        {
            private static readonly ILogger Logger = ParagonLogManager.GetLogger();
            private readonly Action<IntPtr, string> _onWidgetWindowCreated;
            private IntPtr _hHook = IntPtr.Zero;
            private static HookProc _callbackDelegate;
            private uint _threadId;

            public WidgetWindowCreationListener(Action<IntPtr, string> onWidgetWindowCreated)
            {
                _onWidgetWindowCreated = onWidgetWindowCreated;
                Install();
            }

            ~WidgetWindowCreationListener()
            {
                Dispose(false);
            }

            private int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
            {
                if (code < 0)
                {
                    return NativeMethods.CallNextHookEx(_hHook, code, wParam, lParam).ToInt32();
                }
                try
                {
                    if (code == 3 && _onWidgetWindowCreated != null)
                    {
                        var sb = new StringBuilder(256);
                        NativeMethods.GetClassName(wParam, sb, sb.Capacity);
                        _onWidgetWindowCreated(wParam, sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception in hook procedure " + ex);
                }

                return NativeMethods.CallNextHookEx(_hHook, code, wParam, lParam).ToInt32();
            }

            private void Install()
            {
                if (_callbackDelegate != null)
                {
                    throw new InvalidOperationException("Can't install more than one CBT hook");
                }

                if (_hHook == IntPtr.Zero)
                {
                    _threadId = NativeMethods.GetCurrentThreadId();
                    _callbackDelegate = new HookProc(CoreHookProc);
                    _hHook = NativeMethods.SetWindowsHookEx(5 /*CBT*/, _callbackDelegate, IntPtr.Zero, (int)_threadId);
                }
            }

            private void Uninstall()
            {
                if (_callbackDelegate != null & _hHook != IntPtr.Zero)
                {
                    if (!NativeMethods.UnhookWindowsHookEx(_hHook))
                    {
                        throw new Win32Exception();
                    }

                    _hHook = IntPtr.Zero;
                    _callbackDelegate = null;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!disposing)
                {
                    return;
                }

                if (_hHook != IntPtr.Zero)
                {
                    Uninstall();
                }
            }
        }
    }
}