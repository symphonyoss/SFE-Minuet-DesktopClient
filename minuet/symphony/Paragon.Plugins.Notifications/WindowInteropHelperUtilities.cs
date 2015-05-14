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

                try
                {
                    // SafeCreateWindow only exists in the .NET 2.0 runtime. If we try to
                    // invoke this method on the .NET 4.0 runtime it will result in a
                    // MissingMethodException, see below.
                    typeof(Window).InvokeMember(
                        "SafeCreateWindow",
                        BindingFlags.InvokeMethod |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic,
                        null, window, null);
                }
                catch (MissingMethodException)
                {
                    // If we ended up here it means we are running on the .NET 4.0 runtime,
                    // where the method we need to call for the handle was renamed/replaced
                    // with CreateSourceWindow.
                    typeof(Window).InvokeMember(
                        "CreateSourceWindow",
                        BindingFlags.InvokeMethod |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic,
                        null, window, new object[] { false });
                }
            }

            return helper.Handle;
        }
    }
}