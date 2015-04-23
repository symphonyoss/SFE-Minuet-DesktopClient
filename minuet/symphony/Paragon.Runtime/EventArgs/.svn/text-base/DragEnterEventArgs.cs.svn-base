using System.ComponentModel;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    /// <summary>
    /// Provides the data for a drag enter event that has occured in a CEF browser.
    /// </summary>
    public class DragEnterEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Creates a new <see cref="DragEnterEventArgs"/> with Cancel set to false as default.
        /// </summary>
        /// <param name="dragData"></param>
        /// <param name="mask"></param>
        public DragEnterEventArgs(CefDragData dragData, CefDragOperationsMask mask)
            : base(false)
        {
            Data = dragData;
            Effect = mask;
        }

        /// <summary>
        /// Gets the data associated with the event.
        /// </summary>
        public CefDragData Data { get; private set; }

        /// <summary>
        /// The target drop effect.
        /// </summary>
        public CefDragOperationsMask Effect { get; private set; }
    }
}