using System;
using System.Windows.Forms;

namespace Paragon.Runtime.WinForms
{
    internal class Win32Window : IWin32Window
    {
        private readonly IntPtr _handle;

        public Win32Window(IntPtr handle)
        {
            _handle = handle;
        }

        IntPtr IWin32Window.Handle
        {
            get { return _handle; }
        }
    }
}