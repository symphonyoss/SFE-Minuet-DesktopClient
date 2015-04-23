using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

namespace Paragon.Plugins.Notifications
{
    public static class WindowInteropHelperUtilities
    {
        public static IntPtr EnsureHandle(this WindowInteropHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException("helper");
            }

            if (helper.Handle == IntPtr.Zero)
            {
                var window = (Window) typeof (WindowInteropHelper).InvokeMember(
                    "_window",
                    BindingFlags.GetField |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic,
                    null, helper, null);

                typeof (Window).InvokeMember(
                    "SafeCreateWindow",
                    BindingFlags.InvokeMethod |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic,
                    null, window, null);
            }

            return helper.Handle;
        }
    }
}