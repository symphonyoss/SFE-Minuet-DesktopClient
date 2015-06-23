using System;
using System.Windows;
using System.Windows.Interop;
using Paragon.Plugins.Notifications.Win32;

namespace Paragon.Plugins.Notifications.Views
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private readonly IntPtr handle;

        public NotificationWindow()
        {
            InitializeComponent();
            handle = new WindowInteropHelper(this).EnsureHandle();
            Win32Api.AddToolWindowStyle(this.handle);
        }

        public void ShowOnMonitor(IMonitor monitor)
        {
            Win32Api.MoveAndShowWithNoActivate(handle, monitor);
            Show();
        }
    }
}