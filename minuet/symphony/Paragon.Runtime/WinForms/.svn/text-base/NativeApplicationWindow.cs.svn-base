using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

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

        [DebuggerStepThrough]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
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