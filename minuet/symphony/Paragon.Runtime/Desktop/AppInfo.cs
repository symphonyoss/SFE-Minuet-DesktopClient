using System;
using System.Security.Permissions;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Desktop
{
    /// <summary>
    /// Provides cross-process access to key metadata on running apps.
    /// </summary>
    public sealed class AppInfo : IDisposable, IEquatable<AppInfo>, IParagonAppInfo
    {
        private readonly IntPtr _hwnd;
        private bool _disposed;
        private string _workspaceId;

        internal AppInfo(IntPtr hwnd)
        {
            _hwnd = hwnd;
            BrowserPid = (int) Win32Api.GetWindowProcessId(_hwnd);

            string comment;
            if (WindowPropertyStore.GetComment(hwnd, out comment))
            {
                AppWindowProperties props;
                if (AppWindowProperties.TryParse(comment, out props))
                {
                    AppInstanceId = props.AppInstanceId;
                    _workspaceId = props.WorkspaceId;

                    string appId;
                    if (WindowPropertyStore.GetAppId(_hwnd, out appId))
                    {
                        AppId = appId;
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal AppInfo(IntPtr hwnd, string appId, string instanceId, string workspaceId = null)
        {
            _hwnd = hwnd;
            BrowserPid = (int) Win32Api.GetWindowProcessId(_hwnd);
            WindowPropertyStore.SetAppId(hwnd, appId);

            AppInstanceId = instanceId;
            _workspaceId = workspaceId;
            var props = new AppWindowProperties {AppInstanceId = instanceId, WorkspaceId = workspaceId};
            WindowPropertyStore.SetComment(hwnd, props.ToString());
        }

        /// <summary>
        /// Gets performance info for the app's browser process.
        /// </summary>
        public ProcessPerfInfo BrowserInfo { get; private set; }

        /// <summary>
        /// Gets performance info for the app's render process.
        /// </summary>
        public ProcessPerfInfo RenderInfo { get; private set; }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            // If this is instance is owned by a window as opposed to being used to access
            // data from some other window, remove the window properties it is associated with.
            if (BrowserInfo != null)
            {
                BrowserInfo.Dispose();
            }

            _disposed = true;
        }

        public bool Equals(AppInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || _hwnd.Equals(other._hwnd);
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
        /// Gets the ID of the app's browser process.
        /// </summary>
        public int BrowserPid { get; private set; }

        /// <summary>
        /// Gets or sets the identifier for the workspace that the app is associated with.
        /// </summary>
        public string WorkspaceId
        {
            get { return _workspaceId; }

            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                _workspaceId = value;

                string comment;
                if (WindowPropertyStore.GetComment(_hwnd, out comment))
                {
                    AppWindowProperties props;
                    if (AppWindowProperties.TryParse(comment, out props))
                    {
                        props.WorkspaceId = value;
                        WindowPropertyStore.SetComment(_hwnd, props.ToString());
                    }
                }
            }
        }

        public override int GetHashCode()
        {
            return _hwnd.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is AppInfo && Equals((AppInfo) obj);
        }

        public static bool operator ==(AppInfo left, AppInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AppInfo left, AppInfo right)
        {
            return !Equals(left, right);
        }

        internal void InitPerfInfo(int renderPid)
        {
            BrowserInfo = new ProcessPerfInfo(BrowserPid);
            RenderInfo = new ProcessPerfInfo(renderPid);
        }

        public void UpdatePerfInfo(DateTime now)
        {
            if (BrowserInfo != null)
            {
                BrowserInfo.Refresh(now);
            }

            if (RenderInfo != null)
            {
                RenderInfo.Refresh(now);
            }
        }
    }
}