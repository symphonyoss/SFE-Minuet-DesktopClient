using System;
using System.Windows;

namespace Paragon.Runtime.WPF
{
    public partial class JavaScriptDialogWindow
    {
        public JavaScriptDialogWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // TODO: Determine why the window size is not correctly applied for windows with SizeToContent set to something other than Manual.
            SizeToContent = SizeToContent.Manual;
            Height += 1;
            SizeToContent = SizeToContent.Height;
        }
    }
}