using System;
using System.ComponentModel;
using System.Text;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WinForms
{
    internal class WidgetWindowCreationListener : IDisposable
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