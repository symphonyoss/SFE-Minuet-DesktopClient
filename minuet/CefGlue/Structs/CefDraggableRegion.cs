namespace Xilium.CefGlue
{
    using System;
    using Xilium.CefGlue.Interop;

    public sealed unsafe class CefDraggableRegion
    {
        public CefDraggableRegion()
        { }

        public CefRectangle Bounds{ get; set; }

        /// <summary>
        /// True this region is draggable and false otherwise.
        /// </summary>
        public bool Draggable { get; set; }

        internal static CefDraggableRegion FromNative(cef_draggable_region_t* ptr)
        {
            return new CefDraggableRegion
            {
                Bounds = new CefRectangle(ptr->bounds.x, ptr->bounds.y, ptr->bounds.width, ptr->bounds.height),
                Draggable = ptr->draggable != 0
            };
        }

        internal cef_draggable_region_t* ToNative()
        {
            var ptr = cef_draggable_region_t.Alloc();
            ptr->bounds = new cef_rect_t(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
            ptr->draggable = Draggable ? 1 : 0;
            return ptr;
        }

        internal static void Free(cef_draggable_region_t* ptr)
        {
            cef_draggable_region_t.Free((cef_draggable_region_t*)ptr);
        }
    }
}
