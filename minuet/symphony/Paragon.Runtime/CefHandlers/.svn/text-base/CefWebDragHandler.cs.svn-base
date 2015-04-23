using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebDragHandler : CefDragHandler
    {
        private readonly ICefWebBrowserInternal _browser;

        public CefWebDragHandler(ICefWebBrowserInternal browser)
        {
            _browser = browser;
        }

        /// <summary>
        /// Called when an external drag event enters the browser window.
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="dragData">contains the drag event data</param>
        /// <param name="mask">represents the type of drag operation</param>
        /// <returns>Return false for default drag handling behavior or true to cancel the drag event.</returns>
        protected override bool OnDragEnter(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask)
        {
            var eventArgs = new DragEnterEventArgs(dragData, mask);
            _browser.OnDragEnter(eventArgs);
            return eventArgs.Cancel;
        }
    }
}