using System;

namespace Paragon.Plugins
{
    public class DownloadProgressEventArgs : EventArgs
    {
        public DownloadProgressEventArgs(string url, bool isValid, bool isInProgress, bool isComplete, bool isCancelled,
            long currentSpeed, int percentComplete, long totalBytes, long receivedBytes,
            string fullPath, uint id,
            string suggestedFileName, string contentDisposition, string mimeType)
        {
            Cancel = false;
            Url = url;
            IsValid = isValid;
            IsInProgress = isInProgress;
            IsComplete = isComplete;
            IsCanceled = isCancelled;
            CurrentSpeed = currentSpeed;
            PercentComplete = percentComplete;
            TotalBytes = totalBytes;
            ReceivedBytes = receivedBytes;
            FullPath = fullPath;
            Id = id;
            SuggestedFileName = suggestedFileName;
            ContentDisposition = contentDisposition;
            MimeType = mimeType;
        }

        public bool Cancel { get; set; }
        public bool IsValid { get; private set; }
        public bool IsInProgress { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsCanceled { get; private set; }
        public long CurrentSpeed { get; private set; }
        public int PercentComplete { get; private set; }
        public long TotalBytes { get; private set; }
        public long ReceivedBytes { get; private set; }
        public string FullPath { get; private set; }
        public uint Id { get; private set; }
        public string Url { get; private set; }
        public string SuggestedFileName { get; private set; }
        public string ContentDisposition { get; private set; }
        public string MimeType { get; private set; }
    }
}