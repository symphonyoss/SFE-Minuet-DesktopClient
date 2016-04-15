//
// This file manually written from cef/include/internal/cef_types.h.
//
namespace Xilium.CefGlue.Interop
{
    using System;
    using System.Runtime.InteropServices;

    ///
    /// Structure representing a draggable region.
    ///
    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_draggable_region_t
    {
        ///
        /// Bounds of the region.
        ///
        public cef_rect_t bounds;
        ///
        /// True (1) this this region is draggable and false (0) otherwise.
        ///
        public int draggable;

        #region Alloc & Free
        private static int _sizeof;

        static cef_draggable_region_t()
        {
            _sizeof = Marshal.SizeOf(typeof(cef_draggable_region_t));
        }

        public static cef_draggable_region_t* Alloc()
        {
            var ptr = (cef_draggable_region_t*)Marshal.AllocHGlobal(_sizeof);
            *ptr = new cef_draggable_region_t();
            return ptr;
        }

        public static void Free(cef_draggable_region_t* ptr)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }
        #endregion
    }
}
