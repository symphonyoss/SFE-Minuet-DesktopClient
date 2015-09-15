using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebDownloadHandler : CefDownloadHandler
    {
        private readonly ICefWebBrowserInternal _owner;

        public CefWebDownloadHandler(ICefWebBrowserInternal owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Called when a download's status or progress information has been updated.
        /// This may be called multiple times before and after OnBeforeDownload().
        /// Execute |callback| either asynchronously or in this method to cancel the
        /// download if desired. Do not keep a reference to |download_item| outside of
        /// this method.
        /// </summary>
        protected override void OnDownloadUpdated(CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback)
        {
            _owner.OnDownloadUpdated(new DownloadUpdatedEventArgs(downloadItem, callback));
        }

        protected override void OnBeforeDownload(CefBrowser browser, CefDownloadItem downloadItem, string suggestedName, CefBeforeDownloadCallback callback)
        {
            var args = new BeginDownloadEventArgs(downloadItem.Url, downloadItem.MimeType);
            _owner.OnBeforeDownload(args);

            if (string.IsNullOrEmpty(args.DownloadPath))
            {
                callback.Continue(string.Empty, true);
            }
            else
            {
                callback.Continue(args.DownloadPath, false);
            }
        }
    }
}