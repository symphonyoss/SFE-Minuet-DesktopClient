using System;

namespace Paragon.Runtime.Desktop
{
    /// <summary>
    /// Provides access to app related events.
    /// </summary>
    internal static class AppEvents
    {
        /// <summary>
        /// Event fired when an app has exited.
        /// </summary>
        public static event EventHandler<AppEventArgs> AppExited;

        /// <summary>
        /// Event fired when a new instance of an app has been launched.
        /// </summary>
        public static event EventHandler<AppEventArgs> AppLaunched;

        internal static void RaiseAppExited(AppInfo appInfo)
        {
            RaiseAppEvent(appInfo, AppExited);
        }

        internal static void RaiseAppLaunched(AppInfo appInfo)
        {
            RaiseAppEvent(appInfo, AppLaunched);
        }

        private static void RaiseAppEvent(AppInfo appInfo, EventHandler<AppEventArgs> handler)
        {
            if (handler != null)
            {
                handler(null, new AppEventArgs(appInfo));
            }
        }
    }
}