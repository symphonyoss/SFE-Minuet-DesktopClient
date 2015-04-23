using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal interface ICefWebBrowserInternal
    {
        void OnBrowserAfterCreated(CefBrowser browser);
        void OnBeforePopup(BeforePopupEventArgs args);
        void OnCertificateError();
        void OnTitleChanged(TitleChangedEventArgs args);
        void OnLoadEnd(LoadEndEventArgs args);
        void OnLoadError(LoadErrorEventArgs args);
        void OnLoadStart(LoadStartEventArgs e);
        void OnRenderProcessTerminated(RenderProcessTerminatedEventArgs args);
        void OnBeforeContextMenu(ContextMenuEventArgs args);
        void OnContextMenuCommand(ContextMenuCommandEventArgs args);
        void OnDownloadUpdated(DownloadUpdatedEventArgs args);
        bool OnProcessMessageReceived(CefBrowser browser, CefProcessMessage message);
        void OnBeforeResourceLoad(ResourceLoadEventArgs ea);
        void OnJSDialog(JsDialogEventArgs ea);
        void OnBeforeUnloadDialog(UnloadDialogEventArgs ea);
        void OnPreviewKeyEvent(CefKeyEvent keyEvent);
        void OnClosed(CefBrowser browser);

        /// <summary>
        /// Called when an external drag event enters the browser view.
        /// </summary>
        /// <param name="args"></param>
        void OnDragEnter(DragEnterEventArgs args);

        void OnTakeFocus(TakeFocusEventArgs ea);

        void OnProtocolExecution(ProtocolExecutionEventArgs ea);

        bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool isRedirect);
    }
}