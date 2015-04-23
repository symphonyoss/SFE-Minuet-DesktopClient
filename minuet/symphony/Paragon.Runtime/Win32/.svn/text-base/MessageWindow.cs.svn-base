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