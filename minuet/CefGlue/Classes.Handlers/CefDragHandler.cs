namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// Implement this interface to handle events related to dragging. The methods of
    /// this class will be called on the UI thread.
    /// </summary>
    public abstract unsafe partial class CefDragHandler
    {
        private int on_drag_enter(cef_drag_handler_t* self, cef_browser_t* browser, cef_drag_data_t* dragData, CefDragOperationsMask mask)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_dragData = CefDragData.FromNative(dragData);
            var m_result = OnDragEnter(m_browser, m_dragData, mask);

            return m_result ? 1 : 0;
        }

        /// <summary>
        /// Called when an external drag event enters the browser window. |dragData|
        /// contains the drag event data and |mask| represents the type of drag
        /// operation. Return false for default drag handling behavior or true to
        /// cancel the drag event.
        /// </summary>
        protected virtual bool OnDragEnter(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask)
        {
            return false;
        }

        private void on_draggable_regions_changed(cef_drag_handler_t* self, cef_browser_t* browser, UIntPtr regionsCount, cef_draggable_region_t* regions)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_regions = new CefDraggableRegion[(int)regionsCount];

            var count = (int)regionsCount;
            var regionP = regions;
            for (var i = 0; i < count; i++)
            {
                m_regions[i] = CefDraggableRegion.FromNative(regionP);

                regionP++;
            }

            // TODO : Finish this
            OnDraggableRegionsChanged(m_browser, m_regions);
        }

        /// <summary>
        /// Called whenever draggable regions for the browser window change. These can
        /// be specified using the '-webkit-app-region: drag/no-drag' CSS-property. If
        /// draggable regions are never defined in a document this function will also
        /// never be called. If the last draggable region is removed from a document
        /// this function will be called with an NULL vector.
        /// </summary>
        protected virtual void OnDraggableRegionsChanged(CefBrowser browser, CefDraggableRegion[] regions)
        {
        }
    }
}
