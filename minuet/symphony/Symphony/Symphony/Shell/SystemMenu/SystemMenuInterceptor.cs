using System;
using System.Collections.Generic;
using System.Linq;
using Paragon.Plugins;
using Symphony.Win32;

namespace Symphony.Shell.SystemMenu
{
    public class SystemMenuInterceptor
    {
        private readonly IEnumerable<SystemMenuItem> menuItems;
        private IntPtr systemMenu;

        public SystemMenuInterceptor(IEnumerable<SystemMenuItem> menuItems)
        {
            this.menuItems = menuItems;
        }

        public void ApplyToWindow(IApplicationWindow window)
        {
            var nativeWindow = new NativeWindow(window);

            this.systemMenu = NativeMethods.GetSystemMenu(nativeWindow.Handle, false);

            if (this.menuItems.Any())
            {
                NativeMethods.InsertMenu(this.systemMenu, -1, NativeMethods.MF_BYPOSITION | NativeMethods.MF_SEPARATOR, 0, String.Empty);
            }

            foreach (var item in this.menuItems)
            {
                var flags = NativeMethods.MF_BYCOMMAND | NativeMethods.MF_STRING;

                var checkedItem = item as CheckedSystemMenuItem;
                if (checkedItem != null && checkedItem.IsChecked) flags |= NativeMethods.MF_CHECKED;

                NativeMethods.InsertMenu(this.systemMenu, item.Id, flags, (uint)item.Id, item.Header);
            }

            nativeWindow.AddHook(this.WndProc);
        }
        

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((uint)msg)
            {
                case NativeMethods.WM_SYSCOMMAND:
                    var menuItem = this.menuItems.FirstOrDefault(mi => mi.Id == wParam.ToInt32());
                    if (menuItem != null)
                    {
                        menuItem.Command.Execute(menuItem.CommandParameter);
                        handled = true;
                    }

                    break;

                case NativeMethods.WM_INITMENUPOPUP:
                    if (this.systemMenu == wParam)
                    {
                        foreach (var item in this.menuItems)
                        {
                            NativeMethods.EnableMenuItem(
                                this.systemMenu,
                                (uint)item.Id,
                                item.Command.CanExecute(item.CommandParameter) ? NativeMethods.MF_ENABLED : NativeMethods.MF_DISABLED);
                        }

                        foreach (var item in this.menuItems.OfType<CheckedSystemMenuItem>())
                        {
                            NativeMethods.CheckMenuItem(
                                this.systemMenu,
                                (uint)item.Id,
                                item.IsChecked ? NativeMethods.MF_CHECKED : NativeMethods.MF_UNCHECKED);
                        }

                        handled = true;
                    }

                    break;
            }

            return IntPtr.Zero;
        }
    }
}
