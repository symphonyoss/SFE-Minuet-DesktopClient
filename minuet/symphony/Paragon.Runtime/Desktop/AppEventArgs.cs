using System;

namespace Paragon.Runtime.Desktop
{
    /// <summary>
    /// EventArgs for app related events.
    /// </summary>
    internal sealed class AppEventArgs : EventArgs
    {
        internal AppEventArgs(AppInfo appInfo)
        {
            AppInfo = appInfo;
        }

        /// <summary>
        /// Gets metadata on the app that is associated with the event.
        /// </summary>
        public AppInfo AppInfo { get; private set; }
    }
}