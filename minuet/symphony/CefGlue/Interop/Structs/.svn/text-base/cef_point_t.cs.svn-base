namespace Xilium.CefGlue.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_point_t
    {
        public int X;
        public int Y;

        public cef_point_t(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}