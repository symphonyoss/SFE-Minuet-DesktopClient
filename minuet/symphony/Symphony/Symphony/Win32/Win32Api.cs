using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace Symphony.Win32
{
    public static class Win32Api
    {
        public static void ShowWindow(IntPtr handle)
        {
            if (NativeMethods.IsIconic(handle))
            {
                // restore only if minimized
                NativeMethods.ShowWindow(handle, NativeMethods.SW_RESTORE);
            }
            NativeMethods.SetForegroundWindow(handle);
        }

        public static void ShowWithNoActivate(IntPtr handle, Action<RoutedEventHandler> attachToOnLoaded)
        {
            var currentForegroundWindow = NativeMethods.GetForegroundWindow();
            var thisWindowThreadId = NativeMethods.GetWindowThreadProcessId(handle, IntPtr.Zero);
            var currentForegroundWindowThreadId = NativeMethods.GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

            NativeMethods.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

            UInt32 flags = NativeMethods.SWP_NOSIZE
                | NativeMethods.SWP_NOMOVE
                | NativeMethods.SWP_SHOWWINDOW
                | NativeMethods.SWP_NOACTIVATE;

            NativeMethods.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, flags);
            NativeMethods.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);

            attachToOnLoaded((sender, args) =>
            {
                var exStyle = (uint)NativeMethods.GetWindowLong(handle, -20);
                exStyle |= NativeMethods.WS_EX_TOOLWINDOW;

                SetWindowLong(handle, -20, (IntPtr)exStyle);
            });
        }

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
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
