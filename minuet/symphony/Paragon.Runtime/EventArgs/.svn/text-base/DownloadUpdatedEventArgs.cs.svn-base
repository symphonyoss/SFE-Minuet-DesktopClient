using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class DownloadUpdatedEventArgs : EventArgs
    {
        public DownloadUpdatedEventArgs(CefDownloadItem item, CefDownloadItemCallback cb)
        {
            DownloadedItem = item;
            Callback = cb;
        }

        public CefDownloadItem DownloadedItem { get; private set; }
        public CefDownloadItemCallback Callback { get; private set; }
    }
}