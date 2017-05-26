//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32;
using Paragon.Plugins;
using Paragon.Runtime.Desktop;
using Paragon.Runtime.Kernel.Plugins;
using Paragon.Runtime.Kernel.Windowing;
using Paragon.Runtime.PackagedApplication;
using Paragon.Runtime.Plugins;
using Xilium.CefGlue;
    
namespace Paragon.Runtime.Kernel.Applications
{
    public class WebApplication : IApplication
    {
        private readonly Func<ICefWebBrowser> _createNewBrowser;
        private readonly Func<IApplicationWindowEx> _createNewWindow;
        private readonly Func<IApplicationWindowManagerEx> _createWindowManager;
        private readonly TimeSpan _eventPageLaunchTimeout;
        private ManualResetEvent _setCookieEvent = new ManualResetEvent(false);
        private IDisposable _appRegistrationToken;
        private bool _disposed;
        private ICefWebBrowser _eventPageBrowser;
        private Timer _eventPageLaunchTimer;
        private Timer _eventPageUnloadTimer;
        private IBrowserSideMessageRouter _router;
        private PackagedApplicationSchemeHandlerFactory _schemeHandler;
        private bool _sessionEnding;
        // Number of times we have tried to refresh the renderer process
        private int _refreshAttempts;
        private ApplicationState _state = ApplicationState.Created;
        private IApplicationWindowManagerEx _windowManager;
        private RenderSidePluginData _renderPlugins;
        
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public WebApplication(
            IApplicationMetadata metadata,
            int startupTimeout,
            IApplicationPackage package,
            Dictionary<string, object> args,
            // ReSharper disable once ParameterTypeCanBeEnumerable.Local
            IParagonPlugin[] kernelPlugins,
            Func<ICefWebBrowser> newBrowser,
            Func<IApplicationWindowEx> newAppWindow,
            Func<IApplicationWindowManagerEx> newWindowManager)
        {
            Logger = ParagonLogManager.GetLogger();
            Metadata = metadata;
            Package = package;
            Args = args;
            Plugins = new List<IParagonPlugin>(kernelPlugins);
            _createNewWindow = newAppWindow;
            _createNewBrowser = newBrowser;
            _createWindowManager = newWindowManager;
            _eventPageLaunchTimeout = TimeSpan.FromSeconds(startupTimeout);
            _renderPlugins = new RenderSidePluginData() { PackagePath = Package != null ? Package.PackageFilePath : string.Empty, Plugins = new List<ApplicationPlugin>() };
            SystemEvents.SessionEnding += OnSessionEnding;
            ParagonRuntime.RenderProcessInitialize += OnRenderProcessInitialize;
        }

        public string RefreshUrl { get; set; }
        public CefCookieManager CookieManager
        {
            get { return CefCookieManager.GetGlobal(null); }
        }

        public IBrowserSideMessageRouter MessageRouter
        {
            get
            {
                if (_router != null)
                {
                    return _router;
                }

                using (AutoStopwatch.TimeIt("Creating message router"))
                {
                    _router = new BrowserSideMessageRouter();

                    // Register core plugins.
                    if (Metadata.UpdateLaunchStatus != null)
                    {
                        Metadata.UpdateLaunchStatus("Loading paragon kernel ...");
                    }

                    if (Package.Manifest.ApplicationPlugins == null)
                    {
                        return _router;
                    }

                    if (Metadata.UpdateLaunchStatus != null)
                    {
                        Metadata.UpdateLaunchStatus("Loading application plugins ...");
                    }

                    using (AutoStopwatch.TimeIt("Initializing plugins"))
                    {
                        foreach (var plugin in Plugins)
                        {
                            using (AutoStopwatch.WarnIfOverThreshold("Initializing plugin: " + plugin.GetType().Name, 100))
                            {
                                if (_router.PluginManager.AddLocalPlugin(plugin))
                                {
                                    plugin.Initialize(this);
                                }
                            }
                        }
                    }

                    if (Package.Manifest.ApplicationPlugins.Any())
                    {
                        using (AutoStopwatch.TimeIt("Loading app plugins"))
                        {
                            var browserSideAppPlugins = new List<IPluginInfo>();
                            // Register custom plugins requested by the app.
                            foreach (var appPlugin in Package.Manifest.ApplicationPlugins)
                            {
                                if (!appPlugin.Assembly.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase) &&
                                    !appPlugin.RunInRenderer)
                                {
                                    browserSideAppPlugins.Add(appPlugin);
                                }
                                else
                                {
                                    _renderPlugins.Plugins.Add(appPlugin as ApplicationPlugin);
                                }
                            }
                            if (browserSideAppPlugins.Count > 0)
                            {
                                var appPlugins = PackagedPluginAssemblyResolver.LoadManagedPlugins(_router.PluginManager, Package, browserSideAppPlugins);
                                if (appPlugins != null)
                                {
                                    foreach (var plugin in appPlugins)
                                    {
                                        Plugins.Add(plugin);
                                        plugin.Initialize(this);
                                    }
                                }
                            }
                        }
                    }

                    return _router;
                }
            }
        }

        public event EventHandler Closed;

        public event EventHandler<ProtocolInvocationEventArgs> ProtocolInvoke;

        public event EventHandler Launched;

        public event EventHandler<ApplicationExitingEventArgs> Exiting;

        public ILogger Logger { get; private set; }

        public Dictionary<string, object> Args { get; private set; }

        public List<IParagonPlugin> Plugins { get; private set; }

        public IApplicationMetadata Metadata { get; private set; }

        public IApplicationManager ApplicationManager
        {
            get
            {
                return Paragon.Runtime.Kernel.Applications.ApplicationManager.GetInstance();
            }
        }

        public string Name
        {
            get { return Package.Manifest.Name; }
        }

        public IApplicationPackage Package { get; private set; }

        public ApplicationState State
        {
            get { return _state; }

            private set
            {
                _state = value;
                OnStateChanged();
            }
        }

        public IApplicationWindowManager WindowManager
        {
            get
            {
                if (_windowManager != null)
                {
                    return _windowManager;
                }

                _windowManager = _createWindowManager();
                _windowManager.Initialize(this, _createNewWindow, () => _eventPageBrowser);
                _windowManager.NoWindowsOpen += OnWindowManagerNoWindowsOpen;
                _windowManager.CreatingWindow += OnWindowManagerCreatingWindow;
                _windowManager.CreatedWindow += OnWindowManagerCreatedWindow;
                return _windowManager;
            }
        }

        public string WorkspaceId { get; set; }

        public virtual void Close()
        {
            if (State >= ApplicationState.Running)
            {
                State = ApplicationState.Closing;

                _eventPageUnloadTimer = new Timer(s => ParagonRuntime.MainThreadContext.Post(
                    o => CloseEventPage(true), null), null, 1000, -1);

                RaiseExiting();
            }
            else if (_eventPageBrowser != null)
            {
                ParagonRuntime.MainThreadContext.Post(o => CloseEventPage(true), null);
            }
            else
            {
                State = ApplicationState.Closed;
                Dispose();
            }
        }

        public IEnumerable<IParagonAppInfo> GetRunningApps()
        {
            return ParagonDesktop.GetAllApps();
        }

        public void OnProtocolInvoke(string uri)
        {
            ProtocolInvoke.Raise(this, new ProtocolInvocationEventArgs(uri));
        }

        public bool SetCookie(string name, string value, string domain, string path, bool httpOnly, bool secure, DateTime? expires, bool global)
        {
            // Reset the wait event.
            _setCookieEvent.Reset();

            var success = false;
            // Set the cookie on the IO thread.
            CefRuntime.PostTask(CefThreadId.IO, new ActionCallbackTask(() =>
            {
                try
                {
                    // Create a cookie in the Chromium cookie manager.
                    success = CefCookieManager.GetGlobal(null).SetCookie(
                        "http://gs.com",
                        new CefCookie
                        {
                            Name = name,
                            Value = value,
                            Domain = domain,
                            Path = path,
                            HttpOnly = httpOnly,
                            Secure = secure,
                            Expires = expires
                        }, null);
                }
                finally
                {
                    _setCookieEvent.Set();
                }
            }));

            // Wait for the call to SetCookie to complete.
            _setCookieEvent.WaitOne(5000);
            return success;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IApplicationWindow FindWindow(int browserId)
        {
            return Array.Find(WindowManager.AllWindows, aw => aw.ContainsBrowser(browserId));
        }

        public virtual void Launch()
        {
            if (Package == null)
            {
                return;
            }

            var manifest = Package.Manifest;
            if (manifest == null)
            {
                Logger.Error("Packaged application manifest is null");
                return;
            }

            Plugins.Add(new ParagonRuntimePlugin(manifest.NativeServices));

            try
            {
                // https://developer.chrome.com/apps/app_lifecycle:
                // The app runtime loads the event page from a user's desktop and the onLaunch() event is fired.
                // This event tells the event page what windows to launch and their dimensions.
                //
                // When the event page has no executing JavaScript, no pending callbacks, and no open windows, 
                // the runtime unloads the event page and closes the app.
                // Before unloading the event page, the onSuspend() event is fired.
                // This gives the event page opportunity to do simple clean-up tasks before the app is closed.
                State = ApplicationState.Launched;

                // Set the handler so we can do the necessary processing when render process is up and running.
                ParagonRuntime.RenderProcessInitialize += OnRenderProcessInitialize;
                LoadEventPage();
            }
            catch (Exception ex)
            {
                Logger.Error("Error launching application: {0}", ex.Message);
            }
        }

        protected int RenderProcessId
        {
            get
            {
                return _eventPageBrowser != null ? _eventPageBrowser.RenderProcessId : -1;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_windowManager != null)
            {
                _windowManager.NoWindowsOpen -= OnWindowManagerNoWindowsOpen;
                _windowManager.CreatingWindow -= OnWindowManagerCreatingWindow;
                _windowManager.CreatedWindow -= OnWindowManagerCreatedWindow;
                _windowManager = null;
            }

            _schemeHandler = null;

            if (_eventPageLaunchTimer != null)
            {
                _eventPageLaunchTimer.Dispose();
                _eventPageLaunchTimer = null;
            }
            if (_router != null)
            {
                _router.Dispose();
                _router = null;
            }
            if (_eventPageBrowser != null)
            {
                _eventPageBrowser.Dispose();
                _eventPageBrowser = null;
            }

            if (_appRegistrationToken != null)
            {
                _appRegistrationToken.Dispose();
                _appRegistrationToken = null;
            }

            if (_setCookieEvent != null)
            {
                _setCookieEvent.Close();
                _setCookieEvent = null;
            }

            if (_eventPageUnloadTimer != null)
            {
                _eventPageUnloadTimer.Dispose();
                _eventPageUnloadTimer = null;
            }
        }

        protected virtual void OnLaunchTimerExpired(object state)
        {
            // retry 3 times just in case chrome is in weird state. 
            if (_refreshAttempts < 3)
            {
                // Tried to refresh but the window is not up yet. Lets do it one more time. 
                Refresh(RefreshUrl);
            }
            else
            {
            Close();
        }
        }

        private void CloseEventPage(bool closeApp)
        {
            // Fire the closed event
            
            if (_eventPageUnloadTimer != null)
            {
                _eventPageUnloadTimer.Dispose();
                _eventPageUnloadTimer = null;
            }

            if (_eventPageBrowser != null)
            {
                _eventPageBrowser.RenderProcessTerminated -= OnRenderProcessTerminated;
                    
            }

            if (closeApp)
            {
                // Shutdown the window manager as we are closing the app
                if (_windowManager != null)
                {
                    WindowManager.NoWindowsOpen -= OnWindowManagerNoWindowsOpen;
                    _windowManager.Shutdown();
                    _windowManager = null;
                }
                CloseApplication();
            }
        }

        protected void OnClosed()
        {
            if (Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }

            foreach (var plugin in Plugins)
            {
                // Log and continue shutting down other plugins
                try
                {
                    plugin.Shutdown();
                }
                catch (Exception exception)
                {
                    Logger.Error("failed to shutdown kernel plugin because: {1}. Continuing to shut down other plugins...", Package.Manifest.Name, exception);
                }
            }
        }

        private void LoadEventPage()
        {
            if (_eventPageBrowser != null)
            {
                return;
            }

            _eventPageBrowser = _createNewBrowser();
            _eventPageBrowser.BeforeBrowserCreate += OnBeforeEventPageBrowserCreate;
            _eventPageBrowser.LoadEnd += OnEventPageBrowserLoad;
            _eventPageBrowser.RenderProcessTerminated += OnRenderProcessTerminated;

            using (AutoStopwatch.TimeIt("Creating browser control"))
            {
                _eventPageBrowser.CreateControl();
            }

            if (_windowManager != null)
            {
                // Now that a new _eventPageBrowser has been created, let window manager know about it. 
                _windowManager.Initialize(this, _createNewWindow, () => _eventPageBrowser);
        }
        }

        private void OnBeforeEventPageBrowserCreate(object sender, BrowserCreateEventArgs e)
        {
            try
            {
                if (_schemeHandler == null)
                {
                    // Create handler for loading the files within the package
                    _schemeHandler = new PackagedApplicationSchemeHandlerFactory(Package);
                }

                e.Router = MessageRouter;

                if (Metadata.UpdateLaunchStatus != null)
                {
                    Metadata.UpdateLaunchStatus("Loading event page ...");
                }

                // PackagedApplicationSchemeHandlerFactory will create HTML source for EventPage, containing background scripts from the manifest
                ((ICefWebBrowser)sender).Source = _schemeHandler.EventPageUrl;
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating event page browser", ex);
            }
        }

        private void CloseApplication()
        {
            if (_eventPageBrowser != null)
            {
                _eventPageBrowser.Dispose();
                _eventPageBrowser = null;
            }
            if (State != ApplicationState.Closed)
            {
                State = ApplicationState.Closed;
                Dispose(true);
            }
        }

        private void OnRenderProcessTerminated(object sender, RenderProcessTerminatedEventArgs e)
        {
            // Post the message so it can be called on the main UI thread. 
            ParagonRuntime.MainThreadContext.Post(
                    o => ReStartRenderProcess(), null);
        }

        private void ReStartRenderProcess()
        {
            // Trying to re-create render process lets close the current windows etc
            _refreshAttempts++;
            foreach (var w in _windowManager.AllWindows)
            {
                w.CloseWindow();
            }                

            CloseEventPage(false);
            if (_eventPageBrowser != null)
            {
                _eventPageBrowser.Close();
                _eventPageBrowser = null;
            }            
        }

        public void Refresh(string url)
        {
            RefreshUrl = url;
            _eventPageBrowser.SendKillRenderer();
        }

        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            _sessionEnding = true;
            Close();
        }

        private void OnRenderProcessInitialize(object sender, RenderProcessInitEventArgs e)
        {
            var index = 0;
            var pluginInitMessage = MessageRouter.CreatePluginInitMessage();
            if (pluginInitMessage != null)
            {
                e.InitArgs.SetList(index++, pluginInitMessage);
            }

            if (_renderPlugins != null)
            {
                var renderPluginInfo = ParagonJsonSerializer.Serialize(_renderPlugins);
                e.InitArgs.SetString(index, renderPluginInfo);
            }
        }

        /// <summary>
        /// Fired when the application's window manager creates a window.
        /// Cancels the event page launch timeout and sets the application into the Running state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnWindowManagerCreatingWindow(object sender, EventArgs e)
        {
            if (Metadata.UpdateLaunchStatus != null)
            {
                Metadata.UpdateLaunchStatus("Creating main window ...");
            }

            StopLaunchTimeout();

            if (State < ApplicationState.Running)
            {
                State = ApplicationState.Running;
            }
        }

        protected virtual void OnStateChanged()
        {
            switch (_state)
            {
                case ApplicationState.Running:
                    // Do it only for regular loads, not for refresh as it was already done before. 
                    if (_refreshAttempts == 0)
                    {
                    _appRegistrationToken = ParagonDesktop.RegisterApp(Metadata.Id, Metadata.InstanceId);
                    }                    
                    break;

                case ApplicationState.Closed:
                    OnClosed();
                    break;
            }
        }

        protected virtual void OnWindowManagerCreatedWindow(IApplicationWindow w, bool isFirst)
        {
            // Window manager was able to create a new window, refresh has been successful.
            if (isFirst)
            {
                _refreshAttempts = 0;
                RefreshUrl = null;
        }
        }

        private void OnWindowManagerNoWindowsOpen(object sender, EventArgs e)
        {
            if (_refreshAttempts > 0)
            {
                // Refresh in progress and all the windows have died, lets open a new instance
                Launch();
            }
            else
            {
            if (Metadata.UpdateLaunchStatus != null)
            {
                Metadata.UpdateLaunchStatus("No windows created. Shutting down ...");
            }

            Close();
        }
        }

        /// <summary>
        /// Fires the paragon.app.runtime.onLaunched event to JavaScript.
        /// </summary>
        private void OnEventPageBrowserLoad(object sender, LoadEndEventArgs e)
        {
            if (_eventPageBrowser != null)
            {
                _eventPageBrowser.LoadEnd -= OnEventPageBrowserLoad;
            }

            if (e != null && e.Frame.IsMain)
            {
                if (Metadata.UpdateLaunchStatus != null)
                {
                    Metadata.UpdateLaunchStatus("Waiting for window creations ...");
                }

                _eventPageLaunchTimer = new Timer(OnLaunchTimerExpired, null, _eventPageLaunchTimeout, TimeSpan.FromMilliseconds(-1));
                Launched.Raise(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fires the paragon.app.runtime.onExiting event to JavaScript.
        /// </summary>
        private void RaiseExiting()
        {
            Exiting.Raise(this, new ApplicationExitingEventArgs(_sessionEnding));
            // Give a bit of time for the event to fire in JavaScript.
            Thread.Sleep(200);
            CloseEventPage(true);
        }

        private void StopLaunchTimeout()
        {
            if (_eventPageLaunchTimer != null)
            {
                _eventPageLaunchTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
                _eventPageLaunchTimer.Dispose();
                _eventPageLaunchTimer = null;
            }
        }
    }
}