using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Kernel.HotKeys
{
    internal class HotKeyService
    {
        private readonly Dictionary<int, string> _hotKeys = new Dictionary<int, string>();
        private readonly NativeApplicationWindow _nativeApplicationWindow;
        private readonly object _syncRoot = new object();
        private int _id;

        public HotKeyService(IApplicationWindow applicationWindow)
        {
            _nativeApplicationWindow = new NativeApplicationWindow(applicationWindow);
            _nativeApplicationWindow.AddHook(WndProc);
        }

        public bool IsEnabled { get; set; }
        public event EventHandler<HotKeyEventArgs> HotKeyPressed;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Add(string name, ModifierKeys modifiers, Keys keys)
        {
            lock (_syncRoot)
            {
                _id = Interlocked.Increment(ref _id);

                if (!NativeMethods.RegisterHotKey(_nativeApplicationWindow.Handle, _id, (uint) modifiers, (uint) keys))
                {
                    throw new NotSupportedException();
                }

                _hotKeys.Add(_id, name);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Remove(string name)
        {
            lock (_syncRoot)
            {
                var matches = _hotKeys
                    .Where(entry => entry.Value == name)
                    .Select(entry => entry.Key).ToList();

                foreach (var match in matches)
                {
                    if (!NativeMethods.UnregisterHotKey(_nativeApplicationWindow.Handle, match))
                    {
                        throw new NotSupportedException();
                    }

                    _hotKeys.Remove(match);
                }
            }
        }

        [DebuggerStepThrough]
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (IsEnabled && msg == (int)WM.HOTKEY)
            {
                var key = wParam.ToInt32();
                var modifiers = (ModifierKeys) (lParam.ToInt32() & 0xFFFF);
                var keys = (Keys) ((lParam.ToInt32() >> 16) & 0xFFFF);

                string name;
                if (!_hotKeys.TryGetValue(key, out name))
                {
                    name = "UnknownHotKey";
                }

                var args = new HotKeyEventArgs(name, modifiers, keys);
                var onHotKeyPressed = HotKeyPressed;
                if (onHotKeyPressed != null)
                {
                    onHotKeyPressed(this, args);
                }
            }

            return IntPtr.Zero;
        }

        private class NativeApplicationWindow : NativeWindow
        {
            private readonly List<HwndSourceHook> _hooks = new List<HwndSourceHook>();

            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public NativeApplicationWindow(IApplicationWindow applicationWindow)
            {
                var window = (Window) applicationWindow;
                if (!window.IsInitialized)
                {
                    window.SourceInitialized += (sender, args) => AssignHandle(sender as Window);
                }
                else
                {
                    AssignHandle(window);
                }
            }

            public void AddHook(HwndSourceHook hwndSourceHook)
            {
                _hooks.Add(hwndSourceHook);
            }

            [DebuggerStepThrough]
            protected override void WndProc(ref Message m)
            {
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
}