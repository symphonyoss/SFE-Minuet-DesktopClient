using System;

namespace Paragon.Plugins
{
    public class BeginDownloadEventArgs : EventArgs
    {
        public BeginDownloadEventArgs(string uri, string mimeType)
        {
            Uri = uri;
            MimeType = mimeType;
        }

        public string DownloadPath { get; set; }
        public string MimeType { get; private set; }
        public string Uri { get; private set; }
    }
}