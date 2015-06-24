using System;

namespace Paragon.Runtime.Desktop
{
    /// <summary>
    /// EventArgs for app related events.
    /// </summary>
    public sealed class AppEventArgs : EventArgs
    {
        public AppEventArgs(AppInfo appInfo)
        {
            AppInfo = appInfo;
        }

        /// <summary>
        /// Gets metadata on the app that is associated with the event.
        /// </summary>
        public AppInfo AppInfo { get; private set; }
    }
}