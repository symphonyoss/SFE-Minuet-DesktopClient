using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Desktop
{
    /// <summary>
    /// Provides cross-process access to info on all running Paragon apps and open Paragon windows.
    /// </summary>
    internal static class ParagonDesktop
    {
        private static readonly ProcessTable RunningProcessTable;

        static ParagonDesktop()
        {
            if (WindowsVersion.IsWin7OrNewer)
            {
                RunningProcessTable = new ProcessTable();
            }
        }

        /// <summary>
        /// Find running apps based on the specified search criteria.
        /// </summary>
        /// <param name="setOptions">Delegate used to set search criteria</param>
        /// <returns>Apps that match the specified criteria</returns>
        public static IEnumerable<IParagonAppInfo> FindApps(Action<AppSearchOptions> setOptions)
        {
            var opts = new AppSearchOptions();
            setOptions(opts);
            return GetAllApps().Where(opts.IsMatch);
        }

        /// <summary>
        /// Find running app information based on the specified search criteria.
        /// </summary>
        /// <param name="setOptions">Delegate used to set search criteria</param>
        /// <returns>App information that match the specified criteria</returns>
        public static IEnumerable<AppInfo> FindAppInfos(Action<AppSearchOptions> setOptions)
        {
            var opts = new AppSearchOptions();
            setOptions(opts);
            return GetAllAppInfo().Where(opts.IsMatch);
        }

        /// <summary>
        /// Find app windows based on the specified search criteria.
        /// </summary>
        /// <param name="setOptions">Delegate used to set search criteria</param>
        /// <returns>App windows that match the specified criteria</returns>
        public static IEnumerable<WindowInfo> FindWindows(Action<WindowSearchOptions> setOptions)
        {
            var opts = new WindowSearchOptions();
            setOptions(opts);
            return GetAllWindows().Where(opts.IsMatch);
        }

        /// <summary>
        /// Get all running apps.
        /// </summary>
        /// <returns>All running applications</returns>
        public static IEnumerable<IParagonAppInfo> GetAllApps()
        {
            return !WindowsVersion.IsWin7OrNewer
                ? Enumerable.Empty<IParagonAppInfo>() 
                : RunningProcessTable.Apps;
        }

        /// <summary>
        /// Get app information (browser + renderer) 
        /// for all running applications
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AppInfo> GetAllAppInfo()
        {
            return RunningProcessTable.AppInformation;
        }

        /// <summary>
        /// Get all open app windows.
        /// </summary>
        /// <returns>All open app windows</returns>
        public static IEnumerable<WindowInfo> GetAllWindows()
        {
            if (!WindowsVersion.IsWin7OrNewer)
            {
                return Enumerable.Empty<WindowInfo>();
            }

            // Every Paragon app has a hidden message window associated with it with 
            // a ParagonApp window classname. This allows us to quickly build a list 
            // of all running apps without having to resort to cross-process messaging.
            var msgHwnds = Win32Api.GetWindowsByClass("ParagonApp").ToList();

            // Construct a list of AppInfo objects using the ParagonApp window handles
            // retrieved from the above query.
            var apps = msgHwnds.Select(h => new AppInfo(h));

            // Resolve all of the browser process ID's for Paragon apps found via the above query.
            var pids = apps.Select(a => a.BrowserPid).ToArray();

            // Get all windows associated with the browser processes. We need to exclude the
            // ParagonApp windows here as GetProcessWindows will return hidden windows as well.
            return Win32Api.GetProcessWindows(pids)
                .Except(msgHwnds)
                .Select(h => new WindowInfo(h)) 
                .Where(i => i.IsParagonWindow);
        }

        public static IParagonAppInfo GetApp(string instanceId)
        {
            return FindApps(opts => opts.InstanceId = instanceId).FirstOrDefault();
        }

        public static AppInfo GetAppInfo(string instanceId)
        {
            return FindAppInfos(opts => opts.InstanceId = instanceId).FirstOrDefault();
        }

        /// <summary>
        /// Get an app window from a window handle.
        /// </summary>
        /// <param name="hwnd">The window handle</param>
        /// <returns>The app window associated with the handle or null if no window was found</returns>
        public static WindowInfo GetWindow(IntPtr hwnd)
        {
            if (!WindowsVersion.IsWin7OrNewer)
            {
                return null;
            }

            var windowInfo = new WindowInfo(hwnd);
            return windowInfo.IsParagonWindow ? windowInfo : null;
        }

        /// <summary>
        /// Register an instance of an app.
        /// </summary>
        /// <param name="appId">The app identifier</param>
        /// <param name="instanceId">The app instance identifier</param>
        /// <returns>
        /// An <see cref="IDisposable" /> token that should be disposed when the app is closed.
        /// app specific events
        /// </returns>
        public static IDisposable RegisterApp(string appId, string instanceId)
        {
            if (!WindowsVersion.IsWin7OrNewer)
            {
                return new DummyRegistrationToken();
            }

            return new AppRegistrationToken(appId, instanceId);
        }

        /// <summary>
        /// Register an app window.
        /// </summary>
        /// <param name="hwnd">The window handle</param>
        /// <param name="appId">The app identifier</param>
        /// <param name="instanceId">The app instance identifier</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void RegisterAppWindow(IntPtr hwnd, string appId, string instanceId)
        {
            if (!WindowsVersion.IsWin7OrNewer)
            {
                return;
            }

            var props = new AppWindowProperties {AppInstanceId = instanceId};
            WindowPropertyStore.SetComment(hwnd, props.ToString());
            WindowPropertyStore.SetAppId(hwnd, appId);
            WindowPropertyStore.PreventTaskbarPinning(hwnd);
        }

        private class AppRegistrationToken : IDisposable
        {
            private AppInfo _appInfo;
            private MessageWindow _messageWindow;

            public AppRegistrationToken(string appId, string instanceId)
            {
                _messageWindow = new MessageWindow("ParagonApp");
                _appInfo = new AppInfo(_messageWindow.Hwnd, appId, instanceId);
            }

            public void Dispose()
            {
                if (_messageWindow != null)
                {
                    _messageWindow.Dispose();
                    _messageWindow = null;
                }

                if (_appInfo != null)
                {
                    _appInfo.Dispose();
                    _appInfo = null;
                }
            }
        }

        private class DummyRegistrationToken : IDisposable
        {
            public void Dispose()
            {
                // Nothing to do here.
            }
        }

        public class AppSearchOptions
        {
            public string AppId { get; set; }
            public string InstanceId { get; set; }

            internal bool IsMatch(IParagonAppInfo appInfo)
            {
                if (!string.IsNullOrEmpty(AppId)
                    && !AppId.Equals(appInfo.AppId, StringComparison.OrdinalIgnoreCase))
                {
                    // Skip apps that don't match the specified app ID.
                    return false;
                }

                return string.IsNullOrEmpty(InstanceId)
                       || InstanceId.Equals(appInfo.AppInstanceId,
                           StringComparison.OrdinalIgnoreCase);
            }
        }

        private class ProcessTable : IDisposable
        {
            private const int MillisAfterLastAccessToSwitchOffPerfUpdates = 20000;
            private readonly List<AppInfo> _apps = new List<AppInfo>();
            private DateTime _lastAccess;
            private bool _perfRefreshEnabled;
            private Timer _updateTimer;

            public ProcessTable()
            {
                _updateTimer = new Timer(Update, null, Timeout.Infinite, Timeout.Infinite);
                _updateTimer.Change(1000, Timeout.Infinite);
            }

            public IEnumerable<IParagonAppInfo> Apps
            {
                get
                {
                    var apps = _apps.ToArray();
                    _lastAccess = DateTime.UtcNow;

                    // If perf info refresh has been disabled due to lack of access, perform a
                    // refresh now and re-enable it.
                    if (!_perfRefreshEnabled)
                    {
                        apps.ToList().ForEach(a => a.BrowserInfo.Refresh(_lastAccess));
                        _perfRefreshEnabled = true;
                    }

                    return apps;
                }
            }

            public IEnumerable<AppInfo> AppInformation
            {
                get
                {
                    return _apps;
                }
            }

            public void Dispose()
            {
                if (_updateTimer != null)
                {
                    _updateTimer.Dispose();
                    _updateTimer = null;
                }
            }

            private void Update(object state = null)
            {
                try
                {
                    // Get all running apps.
                    var runningApps = Win32Api.GetWindowsByClass(
                        "ParagonApp").Select(h => new AppInfo(h)).ToList();

                    // Determine new instances and instances that have exited.
                    var newApps = runningApps.Except(_apps).ToList();
                    var exitedApps = _apps.Except(runningApps).ToList();

                    // Add/remove to the master list.
                    _apps.RemoveAll(exitedApps.Contains);

                    if (newApps.Count > 0)
                    {
                        var newAppLookup = newApps.ToDictionary(a => a.BrowserPid, a => a);

                        var callback = new Win32Api.EnumChildProcsDelegate(
                            (pid, childPid) => newAppLookup[pid].InitPerfInfo(childPid));

                        Win32Api.EnumChildProcs(callback, newAppLookup.Keys.ToArray());
                        _apps.AddRange(newApps);
                    }

                    // Fire launch events for new apps.
                    newApps.ForEach(AppEvents.RaiseAppLaunched);

                    // Fire exited events for exited apps.
                    exitedApps.ForEach(a =>
                    {
                        AppEvents.RaiseAppExited(a);
                        a.Dispose();
                    });

                    // If perf info refresh is enabled, do an update.
                    if (_perfRefreshEnabled)
                    {
                        // Use UtcNow as it is faster than converting to local (DateTime.Now) and we
                        // are only using the timestamp to determine the time span since the last time
                        // perf info was updated.
                        var now = DateTime.UtcNow;

                        // The "now" timestamp is created once here and passed to each instance to 
                        // save the cost of creating a new one internally.
                        _apps.ForEach(a => a.UpdatePerfInfo(now));

                        // If no queries have been made to the process table after a set amount of
                        // time, disable perf info refresh to save cycles. It will be re-enabled
                        // the first time a new query is made to the process table.
                        if ((now - _lastAccess).TotalMilliseconds > MillisAfterLastAccessToSwitchOffPerfUpdates)
                        {
                            _perfRefreshEnabled = false;
                        }
                    }
                }
                finally
                {
                    _updateTimer.Change(1000, Timeout.Infinite);
                }
            }
        }

        public class WindowSearchOptions
        {
            public WindowSearchOptions()
            {
                WindowsToExclude = new List<IntPtr>();
            }

            public string AppId { get; set; }
            public string InstanceId { get; set; }
            public WindowVisibility? WindowVisibility { get; set; }
            public List<IntPtr> WindowsToExclude { get; private set; }

            internal bool IsMatch(WindowInfo windowInfo)
            {
                if (WindowVisibility.HasValue
                    && WindowVisibility.Value != windowInfo.WindowVisibility)
                {
                    // Skip any windows that don't match the visiblity option.
                    return false;
                }

                if (!string.IsNullOrEmpty(AppId)
                    && !AppId.Equals(windowInfo.AppId, StringComparison.OrdinalIgnoreCase))
                {
                    // Skip windows that don't match the specified app ID.
                    return false;
                }

                return string.IsNullOrEmpty(InstanceId) || InstanceId.Equals(windowInfo.AppInstanceId,
                    StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}