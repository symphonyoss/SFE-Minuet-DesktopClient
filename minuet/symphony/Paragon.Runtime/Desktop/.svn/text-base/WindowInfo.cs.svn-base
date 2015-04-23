using System;
using System.Security.Permissions;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Desktop
{
    /// <summary>
    /// Provides cross-process access to key metadata on app windows.
    /// </summary>
    internal sealed class WindowInfo
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal WindowInfo(IntPtr hwnd)
        {
            Hwnd = hwnd;

            string comment;
            if (WindowPropertyStore.GetComment(hwnd, out comment))
            {
                AppWindowProperties props;
                IsParagonWindow = AppWindowProperties.TryParse(comment, out props);
                if (IsParagonWindow)
                {
                    AppInstanceId = props.AppInstanceId;

                    string appId;
                    if (WindowPropertyStore.GetAppId(hwnd, out appId))
                    {
                        AppId = appId;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the identifier string for the app as specified in the manifest.
        /// </summary>
        public string AppId { get; private set; }

        /// <summary>
        /// Gets the app instance identifier assigned by Paragon.
        /// </summary>
        public string AppInstanceId { get; private set; }

        /// <summary>
        /// Gets the window bounds for the associated window.
        /// </summary>
        public RECT Bounds
        {
            get { return RECT.FromHandle(Hwnd); }
        }

        /// <summary>
        /// Gets the handle for the associated window.
        /// </summary>
        public IntPtr Hwnd { get; private set; }

        /// <summary>
        /// Gets the visibility of the associated window.
        /// </summary>
        public WindowVisibility WindowVisibility
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get { return Win32Api.GetWindowVisibility(Hwnd); }
        }

        internal bool IsParagonWindow { get; private set; }
    }
}