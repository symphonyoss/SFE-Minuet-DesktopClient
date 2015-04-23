using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Paragon.Plugins;

namespace Symphony.Shell
{
    public class NativeWindow : System.Windows.Forms.NativeWindow
    {
        private readonly List<HwndSourceHook> hooks = new List<HwndSourceHook>();
        private readonly Window window;

        public NativeWindow(IApplicationWindow applicationWindow)
        {
            this.window = applicationWindow.Unwrap();

            if (!this.window.IsInitialized)
            {
                this.window.SourceInitialized += (sender, args) => this.AssignHandle(sender as Window);
            }
            else
            {
                this.AssignHandle(this.window);
            }
        }

        public void AddHook(HwndSourceHook hwndSourceHook)
        {
            this.hooks.Add(hwndSourceHook);
        }

        //public IntPtr EnsureHandle()
        //{
        //    return new WindowInteropHelper(this.window).EnsureHandle();
        //}

        protected override void WndProc(ref Message m)
        {
            foreach (var hook in this.hooks.ToArray())
            {
                bool handled = false;

                m.Result = hook.Invoke(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);

                if (handled) return;
            }

            base.WndProc(ref m);
        }

        private void AssignHandle(Window window)
        {
            AssignHandle(new WindowInteropHelper(window).Handle);
        }
    }
}
