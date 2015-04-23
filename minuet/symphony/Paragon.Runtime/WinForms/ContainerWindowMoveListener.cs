using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Windows.Forms;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WinForms
{
    internal class ContainerWindowMoveListener : NativeWindow, IDisposable
    {
        private readonly Action _onMove;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public ContainerWindowMoveListener(IntPtr handle, Action onMove)
        {
            _onMove = onMove;
            var h = Win32Api.GetToplevelParent(handle);
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
                ReleaseHandle();
                GC.SuppressFinalize(this);
            }
        }

        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int) WM.MOVE ||
                m.Msg == (int) WM.MOVING)
            {
                if (_onMove != null)
                {
                    _onMove();
                }
            }
            base.WndProc(ref m);
        }
    }
}