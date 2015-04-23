using System;
using Paragon.Plugins;
using Symphony.Configuration;
using Symphony.Mvvm;
using Symphony.Win32;

namespace Symphony.Shell.SystemMenu.MenuItems
{
    public class MinimizeOnClose : CheckedSystemMenuItem
    {
        private readonly IApplicationWindow applicationWindow;
        private readonly ApplicationSettings settings;

        public MinimizeOnClose(
            IApplicationWindow applicationWindow,
            ApplicationSettings settings)
        {
            this.applicationWindow = applicationWindow;
            this.settings = settings;

            this.Id = 102;
            this.Header = "Minimize on Close";
            this.Command = new DelegateCommand(this.OnExecute);
            this.IsChecked = settings.GetIsMinimizeOnClose();

            var nativeWindow = new NativeWindow(applicationWindow);
            nativeWindow.AddHook(this.WndProc);
        }
        
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (this.IsChecked && msg == NativeMethods.WM_CLOSE)
            {
                this.applicationWindow.Minimize();
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void OnExecute()
        {
            this.IsChecked = !this.IsChecked;
            this.settings.Save(this.IsChecked);
        }
    }
}