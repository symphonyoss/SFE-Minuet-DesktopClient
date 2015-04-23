using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WinForms
{
    internal class WidgetWindow : NativeWindow, IDisposable
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
                case (int) WM.DESTROY:
                    Dispose();
                    break;

                case (int) WM.WINDOWPOSCHANGING:
                {
                    var style = Win32Api.GetStyle(Handle);
                    if ((style & (long) WS.POPUP) == (long) WS.POPUP)
                    {
                        Logger.Info(string.Format("Setting popup window {0} created on thread {1} to be top-most", m.HWnd, Win32Api.GetWindowThreadId(m.HWnd)));
                        var pos = (WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof (WINDOWPOS));
                        pos.hwndInsertAfter = new IntPtr(-1);
                        pos.flags = (uint) (pos.flags & ~0x0004);
                        Marshal.StructureToPtr(pos, m.LParam, true);
                    }
                }
                    break;
            }

            base.WndProc(ref m);
        }
    }
}