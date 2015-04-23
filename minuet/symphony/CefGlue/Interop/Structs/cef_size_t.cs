namespace Xilium.CefGlue.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_size_t
    {
        public int Width;
        public int Height;

        public cef_size_t(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}