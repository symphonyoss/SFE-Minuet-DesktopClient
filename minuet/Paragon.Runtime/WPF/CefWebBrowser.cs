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
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Paragon.Plugins;
using Paragon.Runtime.Plugins;
using Paragon.Runtime.Win32;
using Paragon.Runtime.WinForms;
using Xilium.CefGlue;
using System.Linq;
using Paragon.Runtime.PackagedApplication;
using System.Threading;
using System.Windows.Interop;
using System.Web;
using System.Collections.Specialized;

namespace Paragon.Runtime.WPF
{
    public class CefWebBrowser : BrowserHwndHost, ICefWebBrowser, ICefWebBrowserInternal
    {
        private const string DevToolsUrlPrefix = "chrome-devtools://devtools/";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly List<string> _allowedProtocols = new List<string>();

        private readonly int _parentBrowserId = -1;
        private CefBrowser _browser;
        private IntPtr _browserWindowHandle;
        private CefWebClient _client;
        private ContainerWindowMoveListener _containerWindowMoveListener;
        private string _currentUrl = "about:blank";
        private bool _disableContextMenu;
        private bool _disablePopups;
        private WidgetWindowZOrderHandler _widgetWindowZOrderHandler;
        private bool _controlCreated;
        private bool _mainFrameResourceNotAccessible;
        private bool _mainFrameCertificateError;
        private string _name = Guid.NewGuid().ToString();
        private IBrowserSideMessageRouter _router;
        private string _sourceUrl = "about:blank";

        public event EventHandler<BrowserCreateEventArgs> BeforeBrowserCreate;
        public event EventHandler<ContextMenuEventArgs> BeforeContextMenu;
        public event EventHandler<BeginDownloadEventArgs> BeforeDownload;
        public event EventHandler<BeforePopupEventArgs> BeforePopup;
        public event EventHandler<ResourceLoadEventArgs> BeforeResourceLoad;
        public event EventHandler<UnloadDialogEventArgs> BeforeUnloadDialog;
        public event EventHandler BrowserAfterCreated;
        public event EventHandler BrowserClosed;
        public event EventHandler<ContextMenuCommandEventArgs> ContextMenuCommand;
        public event EventHandler<DownloadProgressEventArgs> DownloadUpdated;
        public event EventHandler<JsDialogEventArgs> JSDialog;
        public event EventHandler<LoadEndEventArgs> LoadEnd;
        public event EventHandler<LoadErrorEventArgs> LoadError;
        public event EventHandler<RenderProcessTerminatedEventArgs> RenderProcessTerminated;
        public event EventHandler<ShowPopupEventArgs> ShowPopup;
        public event EventHandler<TitleChangedEventArgs> TitleChanged;
        public event EventHandler<ProtocolExecutionEventArgs> ProtocolExecution;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public CefWebBrowser()
        {
            KeyboardNavigation.SetIsTabStop(this, false);
            KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.Cycle);
            _disablePopups = false;
            _disableContextMenu = false;
            Loaded += OnLoaded;
            ParagonRuntime.AddBrowser(this);
            _allowedProtocols.AddRange(CefBrowserApplication.AllowedProtocols);
        }

        protected CefWebBrowser(int parentId, IBrowserSideMessageRouter router)
            : this()
        {
            _parentBrowserId = parentId;
            _router = router;
        }

        public override IntPtr BrowserWindowHandle
        {
            get
            {
                return _browserWindowHandle;
            }
        }

        public int RenderProcessId{ get; private set; }

        private bool IsPopup
        {
            get { return _parentBrowserId > 0 || (_browser != null && _browser.IsPopup); }
        }

        private bool DesignMode
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(this);
            }
        }

        public string BrowserName
        {
            get { return _name; }
        }
        
        public int Identifier
        {
            get { return _browser != null ? _browser.Identifier : -1; }
        }

        public string Source
        {
            get { return _browser.GetMainFrame().Url; } // Get the actual URL where we are. 
            set
            {
                if (!_sourceUrl.Equals(value))
                {
                    _sourceUrl = value;
                    if (!DesignMode)
                    {
                        NavigateTo(_sourceUrl);
                    }
                }
            }
        }

        public void BlurBrowser()
        {
            if (_browser != null)
            {
                _browser.GetHost().SetFocus(false);
            }
        }

        public void SendKillRenderer()
        {
            _router.SendKillRenderer(_browser);
        }

        public void Close(bool force = false)
        {
            Logger.Info("Closing browser {0}", Identifier);
            if (_browser != null)
            {
                _browser.GetHost().CloseBrowser(force);
            }
        }

        public void ExecuteJavaScript(string script)
        {
            using (var frame = _browser.GetMainFrame())
            {
                frame.ExecuteJavaScript(script, string.Empty, 1);
            }
        }

        public void FocusBrowser()
        {
            if (_browser != null)
            {
                _browser.GetHost().SetFocus(true);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            if (_containerWindowMoveListener == null)
            {
                _containerWindowMoveListener = new ContainerWindowMoveListener(_browserWindowHandle, () => _browser.GetHost().NotifyMoveOrResizeStarted());
            }
        }

        public IDisposable GetDeveloperToolsControl(CefPoint element, CefWindowInfo info = null, CefBrowserSettings settings = null)
        {
            if (_browser != null)
            {
                var ctl = new DevToolsWebBrowser();

                ctl.Disposed += (sender, args) =>
                {
                    if (_browser != null)
                    {
                        _browser.GetHost().CloseDevTools();
                    }
                };

                ctl.CreateControl();

                if (info == null)
                {
                    info = CefWindowInfo.Create();
                    info.Width = (int)ActualWidth;
                    info.Height = (int)ActualHeight;
                }
                if (ctl.ParentHandle != IntPtr.Zero)
                {
                    info.SetAsChild(ctl.ParentHandle, new CefRectangle(0, 0, info.Width, info.Height));
                }

                if (settings == null)
                {
                    settings = new CefBrowserSettings();
                }

                if (element.X > int.MinValue && element.Y > int.MinValue)
                {
                    _browser.GetHost().ShowDevTools(info, ctl.Client, settings, element);
                }
                else
                {
                    var defaultCefPoint = new CefPoint(0, 0);
                    _browser.GetHost().ShowDevTools(info, ctl.Client, settings, defaultCefPoint);
                }

                return ctl;
            }

            return null;
        }

        public void Reload(bool ignoreCache)
        {
            if (ignoreCache)
            {
                if (_browser != null)
                {
                    _browser.ReloadIgnoreCache();
                }
            }
            else
            {
                if (_browser != null)
                {
                    _browser.Reload();
                }
            }
        }

        public void SetTopMost(bool set)
        {
            if (_widgetWindowZOrderHandler != null)
            {
                _widgetWindowZOrderHandler.SetTopMost(set);
            }
        }

        public override void CreateControl()
        {
            base.CreateControl();

            if (!_controlCreated && !DesignMode)
            {
                if (!ParagonRuntime.IsInitialized)
                {
                    ParagonRuntime.Initialize();
                }

                _client = new CefWebClient(this);

                if (!IsPopup && _browser == null)
                {
                    var settings = new CefBrowserSettings();

                    using (AutoStopwatch.TimeIt("Creating browser"))
                    {
                        var info = CefWindowInfo.Create();
                        var ea = new BrowserCreateEventArgs();

                        using (AutoStopwatch.TimeIt("Raising BeforeBrowserCreate event"))
                        {
                            BeforeBrowserCreate.Raise(this, ea);
                        }

                        _router = ea.Router;
                        _currentUrl = _sourceUrl;

                        if (IntPtr.Zero != ParentHandle)
                        {
                            RECT rect = new RECT();
                            Win32Api.GetClientRect(ParentHandle, ref rect);
                            info.SetAsChild(ParentHandle, new CefRectangle(rect.Left, rect.Top, rect.Width, rect.Height));
                        }

                        Logger.Info(string.Format("OnHandleCreated - Creating a browser with url {0}", _currentUrl));
                        CefBrowserHost.CreateBrowser(info, _client, settings, _currentUrl);
                    }
                }

                _controlCreated = true;
            }
        }

        public void SetZoomLevel(double level)
        {
            if (_browser != null)
            {
                _browser.GetHost().SetZoomLevel(level);
            }
        }

        public void RunFileDialog(CefFileDialogMode mode, string title, string defaultFileName, string[] acceptTypes, int selectedAcceptFilter, CefRunFileDialogCallback callback)
        {
            if (_browser != null)
            {
                _browser.GetHost().RunFileDialog(mode, title, defaultFileName, acceptTypes, selectedAcceptFilter, callback);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_browser != null && disposing)
            {
                ParagonRuntime.RemoveBrowser(this);
                if (_widgetWindowZOrderHandler != null)
                {
                    _widgetWindowZOrderHandler.Dispose();
                    _widgetWindowZOrderHandler = null;
                }
                if (_containerWindowMoveListener != null)
                {
                    _containerWindowMoveListener.Dispose();
                    _containerWindowMoveListener = null;
                }
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
                if (_router != null)
                {
                    // Router should not be disposed here since it is shared by all cefwebbrowsers
                    //_router.Dispose();
                    _router = null;
                }
                if (_browser != null)
                {
                    _browser.Dispose();
                    _browser = null;
                }
                _browserWindowHandle = IntPtr.Zero;
            }
        }

        #region ICefWebBrowserInternal implementation
        void ICefWebBrowserInternal.OnBeforeContextMenu(ContextMenuEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            // However, we shall fire this on the IO thread, because the Model is a value type and changes made in the
            // UI thread will be made on a copy

            try
            {
                if (!_disableContextMenu && BeforeContextMenu != null )
                {
                    BeforeContextMenu(this, ea);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in OnBeforeContextMenu : {0}", ex);
            }
            finally
            {
                if (!ea.Handled && _disableContextMenu)
                {
                    ea.Model.Clear();
                }
            }
        }

        public void OnBeforeDownload(BeginDownloadEventArgs args)
        {
            BeforeDownload.Raise(this, args);
        }

        RECT? _initialWindowPlacement;
        public RECT? initialWindowPlacement
        {
            get { return _initialWindowPlacement; }
            internal set { _initialWindowPlacement = value; }
        }

        void ICefWebBrowserInternal.OnBeforePopup(BeforePopupEventArgs e)
        {
            try
            {
                var beforePopup = BeforePopup;
                if (!_disablePopups && beforePopup != null)
                {
                    // If no initial size specified, default to the parent window's size
                    if (e.WindowInfo.Width <= 0 && ActualWidth != double.NaN)
                    {
                        e.WindowInfo.Width = (int)ActualWidth;
                    }
                    if (e.WindowInfo.Height <= 0 && ActualHeight != double.NaN)
                    {
                        e.WindowInfo.Height = (int)ActualHeight;
                    }

                    beforePopup(this, e);

                    if (!e.Cancel && e.NeedCustomPopupWindow)
                    {
                        var name = e.TargetFrameName;
                        
                        // The following code must be executed on the UI thread of the browser process
                        DispatchIfRequired(() => 
                        { 
                            var newBrowser = CreatePopupBrowser(Identifier, _router); 
                            if (newBrowser != null)
                            {
                                // extract x, y from url parameters
                                NameValueCollection query = HttpUtility.ParseQueryString(e.TargetUrl);
                                int x;
                                if (!Int32.TryParse(query["x"], out x))
                                    x = -1;

                                int y;
                                if (!Int32.TryParse(query["y"], out y))
                                    y = -1;
                                
                                if (x != -1 && y != -1)
                                {
                                    var rect = new RECT();
                                    rect.Left = x;
                                    rect.Top = y;
                                    rect.Right = x + e.WindowInfo.Width;
                                    rect.Bottom = y + e.WindowInfo.Height;
                                    newBrowser.initialWindowPlacement = rect;
                                }
                                else
                                    newBrowser.initialWindowPlacement = null;

                                newBrowser.Source = e.TargetUrl;
                                newBrowser._currentUrl = e.TargetUrl;
                                newBrowser.Width = e.WindowInfo.Width;
                                newBrowser.Height = e.WindowInfo.Height;
                                newBrowser._name = name;
                                // Force the creation of the control handle
                                newBrowser.CreateControl();

                                e.WindowInfo.SetAsChild(newBrowser.ParentHandle, new CefRectangle(0, 0, e.WindowInfo.Width, e.WindowInfo.Height));

                                // Set the client to new browser's client so that the OnAfterBrowserCreated (and other notifications) go to that control
                                e.Client = newBrowser._client;
                            };
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                Logger.Error("Error in OnBeforePopup : {0}. Popup will not be allowed.", ex);
            }
            finally
            {
                if (!e.Cancel && _disablePopups)
                {
                    e.Cancel = true;
                }
            }
        }

        void ICefWebBrowserInternal.OnCertificateError()
        {
            _mainFrameCertificateError = true;
        }

        void ICefWebBrowserInternal.OnBeforeResourceLoad(ResourceLoadEventArgs ea)
        {
            // Invoke synchronously because the ResourceLoadEventArgs.Cancel property is read
            // by callers to determine whether the resource should be loaded or not.
            try
            {
                if (!IsResourceAccessible(ea.Url))
                {
                    // Give the container a chance to override
                    if( BeforeResourceLoad != null )
                        BeforeResourceLoad(this, ea);

                    if (!ea.Cancel)
                    {
                        // The URL is whitelisted by the application which takes preference. OK to load.
                        return;
                    }

                    // Requested resource is not accessible. It may not be sourced from one of the
                    // whitelisted domains, for example. Log this info to the paragon and dev tools logs.
                    ea.Cancel = true;
                    var msg = "Resource not accessible: " + ea.Url;
                    ExecuteJavaScript("window.console.error('" + msg + "');");
                    Logger.Error(msg);

                    // If the resource is the main frame, set a flag for later use.
                    if (ea.ResourceType == CefResourceType.MainFrame)
                    {
                        _mainFrameResourceNotAccessible = true;
                    }
                }
                else if (!_allowedProtocols.Contains(new Uri(ea.Url).Scheme))
                {
                    // The protocol scheme for the requested resource is not in the list
                    // of allowed protocol schemes. Cancel the request and log the failure.
                    ea.Cancel = true;
                    var msg = "Resource not accessible due to unknown protocol scheme: " + ea.Url;
                    ExecuteJavaScript("window.console.error('" + msg + "');");
                    Logger.Error(msg);
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Error in OnBeforeResourceLoad : {0}. Resource loading will be aborted.", ex);
                ea.Cancel = true;
            }
        }

        void ICefWebBrowserInternal.OnBeforeUnloadDialog(UnloadDialogEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            if( BeforeUnloadDialog != null )
            {
                DispatchIfRequired(() =>
                {
                    try
                    {
                        BeforeUnloadDialog(this, ea);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error in OnBeforeUnloadDialog : {0}. Dialog will be suppressed.", ex);
                    }
                });
            }
        }

        void ICefWebBrowserInternal.OnBrowserAfterCreated(CefBrowser browser)
        {
            this.DispatchIfRequired(() =>
            {
                try
                {
                    if (_browser != null)
                    {
                        return;
                    }

                    _browser = browser;
                    using (var browserHost = _browser.GetHost())
                    {
                        _browserWindowHandle = browserHost.GetWindowHandle();
                        if (_browserWindowHandle != IntPtr.Zero)
                        {
                            OnHandleCreated();
                            _widgetWindowZOrderHandler = new WidgetWindowZOrderHandler(_browserWindowHandle);
                        }
                    }

                    if (!IsPopup)
                    {
                        if( BrowserAfterCreated != null )
                            BrowserAfterCreated(this, EventArgs.Empty);
                    }
                    else
                    {
                        var parent = ParagonRuntime.FindBrowser<CefWebBrowser>(_parentBrowserId);
                        if (parent == null)
                        {
                            Close();
                        }
                        else
                        {
                            var ea = parent.RaiseShowPopup(this);
                            if (ea == null || !ea.Shown)
                            {
                                Close();
                            }
                        }
                    }

                    SetBrowserZoomLevel();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in OnBrowserAfterCreated : {0}", ex);
                    throw;
                }
            }, true);
        }

        void ICefWebBrowserInternal.OnClosed(CefBrowser browser)
        {
            InvokeHandler(BrowserClosed, EventArgs.Empty, true);
        }

        void ICefWebBrowserInternal.OnContextMenuCommand(ContextMenuCommandEventArgs ea)
        {
            // Raised on the CEF UI thread
            try
            {
                if (ContextMenuCommand != null)
                {
                    ContextMenuCommand(this, ea);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in OnBeforeContextMenu : {0}", ex);
            }
        }

        void ICefWebBrowserInternal.OnDownloadUpdated(DownloadUpdatedEventArgs e)
        {
            if (DownloadUpdated != null)
            {
                var di = e.DownloadedItem;
                var ea = new DownloadProgressEventArgs(di.Url, di.IsValid, di.IsInProgress, di.IsComplete, di.IsCanceled,
                    di.CurrentSpeed, di.PercentComplete, di.TotalBytes, di.ReceivedBytes,
                    di.FullPath, di.Id, di.SuggestedFileName, di.ContentDisposition, di.MimeType);
                InvokeHandler(DownloadUpdated, ea);
                if (ea.Cancel)
                {
                    e.Callback.Cancel();
                }
            }
        }

        void ICefWebBrowserInternal.OnJSDialog(JsDialogEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.DispatchIfRequired(() =>
            {
                try
                {
                    if( JSDialog != null )
                        JSDialog(this, ea);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in OnJSDialog : {0}. Dialog will be suppressed.", ex);
                    ea.SuppressMessage = true;
                }
                finally
                {
                    // Tell CEF we handled the dialog
                    ea.Handled = true;
                }
            });
        }

        void ICefWebBrowserInternal.OnLoadEnd(LoadEndEventArgs e)
        {
            // Reset the flags used to indicate that the resource associated 
            // with the main frame is not acessible or has a cert error.
            _mainFrameResourceNotAccessible = false;
            _mainFrameCertificateError = false;

            this.DispatchIfRequired(() =>
            {
                try
                {
                    // If the source URI has changed since navigation was started, initiate naviagation again.
                    if (!_sourceUrl.Equals(_currentUrl))
                    {
                        NavigateTo(_sourceUrl);
                    }

                    ExecuteJavaScript(StaticWebContent.GetOnLoadEndScript());
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in OnLoadEnd : {0}", ex);
                }
                finally
                {
                    if( LoadEnd != null )
                        LoadEnd(this, e);
                }
            },
            true);
        }

        void ICefWebBrowserInternal.OnLoadError(LoadErrorEventArgs e)
        {
            // OnLoadError is called when an error occurs while loading the main frame 
            // resource (resource of type CefResourceType.MainFrame). OnLoadError is not
            // called when child resources of the main frame (scripts, etc) fail to load.
            this.DispatchIfRequired(() =>
            {
                try
                {
                    if (_mainFrameCertificateError)
                    {
                        var html = StaticWebContent.GetInvalidCertErrorPage(e.FailedUrl);
                        e.Frame.LoadString(html, PackagedApplicationSchemeHandlerFactory.ErrorPage);
                        Logger.Error("Unable to load the main frame due to an invalid certifiate: " + e.FailedUrl);
                    }
                    else if (_mainFrameResourceNotAccessible)
                    {
                        // The resource associated with the main frame is not accessible.
                        // The domain may not be whitelisted, etc.
                        var html = StaticWebContent.GetResourceNotAccessibleErrorPage(e.FailedUrl);
                        e.Frame.LoadString(html, PackagedApplicationSchemeHandlerFactory.ErrorPage);
                        Logger.Error("Main frame resource not accessible: " + e.FailedUrl);
                    }
                    else if (e.ErrorCode != CefErrorCode.Aborted)
                    {
                        // The main frame failed to load for some reason. Show the default error
                        // page along with any error text that may have been provided.
                        var html = StaticWebContent.GetLoadErrorPage(e.FailedUrl, e.ErrorText, ParagonLogManager.CurrentParagonLogFile);
                        e.Frame.LoadString(html, PackagedApplicationSchemeHandlerFactory.ErrorPage);
                        Logger.Error("Error loading the main frame: url = {0}, Error = {1} {2}", e.FailedUrl, e.ErrorCode, e.ErrorText);
                    }
                    else
                    {
                        // In various situations, OnLoadError is called with CefErrorCode.Aborted. Aside
                        // from certificate errors or cases where the main frame resource is not available
                        // (both of which are handled above and both of which are CefErrorCode.Aborted errors) 
                        // OnLoadError may be called for cases where there it does not actually indicate
                        // that a real problem has occurred. For example, it is called when the main frame
                        // is refreshed. We log it here at INFO because in most cases it does not represent
                        // a real error condition.
                        Logger.Info("Main frame load aborted: " + e.FailedUrl);
                    }
                    if (LoadError != null)
                        LoadError(this, e);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception encountered while handling a page load error", ex);
                }
            }, true);
        }

        void ICefWebBrowserInternal.OnLoadStart(LoadStartEventArgs e)
        {
            this.DispatchIfRequired(() =>
            {
                try
                {
                    ExecuteJavaScript(StaticWebContent.GetOnLoadStartScript());
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in OnLoadStart : {0}", ex);
                }
            },
            true);
        }

        void ICefWebBrowserInternal.OnPreviewKeyEvent(CefKeyEvent keyEvent)
        {
            this.DispatchIfRequired(() =>
            {
                if (keyEvent.EventType == CefKeyEventType.RawKeyDown)
                {
                    var keys = (Keys)keyEvent.WindowsKeyCode;
                    if ((keyEvent.Modifiers & CefEventFlags.ControlDown) == CefEventFlags.ControlDown)
                    {
                        keys |= Keys.Control;
                    }
                    if ((keyEvent.Modifiers & CefEventFlags.ShiftDown) == CefEventFlags.ShiftDown)
                    {
                        keys |= Keys.Shift;
                    }
                    if ((keyEvent.Modifiers & CefEventFlags.AltDown) == CefEventFlags.AltDown)
                    {
                        keys |= Keys.Alt;
                    }
                    var source = PresentationSource.FromDependencyObject(this);
                    var key = KeyInterop.KeyFromVirtualKey((int)keys);
                    var args = new System.Windows.Input.KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = PreviewKeyDownEvent };
                    RaiseEvent(args);
                }
            }, true);
        }

        bool ICefWebBrowserInternal.OnProcessMessageReceived(CefBrowser browser, CefProcessMessage message)
        {
            if (CefWebRenderProcessHandler.RENDER_PROC_ID_MESSAGE.Equals(message.Name, StringComparison.CurrentCultureIgnoreCase))
            {
                RenderProcessId = message.Arguments.GetInt(0);
            }
            else if(_router != null)
                return _router.ProcessCefMessage(browser, message);
            return false;
        }

        void ICefWebBrowserInternal.OnRenderProcessTerminated(RenderProcessTerminatedEventArgs e)
        {
            if (RenderProcessTerminated != null)
                RenderProcessTerminated(this, e);
            else
            {
                var frame = e.Browser.GetMainFrame();
                frame.LoadUrl(frame.Url);
            }
        }

        void ICefWebBrowserInternal.OnTitleChanged(TitleChangedEventArgs e)
        {
            InvokeHandler(TitleChanged, e, true);
        }

        void ICefWebBrowserInternal.OnProtocolExecution(ProtocolExecutionEventArgs ea)
        {
            // Raised on CEF UI thread. 
            try
            {
                if( ProtocolExecution != null )
                    ProtocolExecution(this, ea);
                if (ea.Allow)
                {
                    var scheme = new Uri(ea.Url).Scheme;
                    if (!_allowedProtocols.Contains(scheme))
                        _allowedProtocols.Add(scheme);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in OnProtocolExecution : {0}. Protocol will not be allowed.", ex.Message);
                ea.Allow = false;
            }
        }

        bool ICefWebBrowserInternal.OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool isRedirect)
        {
            string scheme = null;
            try
            {
                scheme = new Uri(request.Url).Scheme;
            }
            catch
            {
                return false;
            }

            return !string.IsNullOrEmpty(scheme) && !CefBrowserApplication.AllowedProtocols.Contains(scheme);
        }

        bool ICefWebBrowserInternal.OnGetAuthCredentials(CefBrowser browser, CefFrame frame, bool isProxy, string host, int port, string realm, string scheme, CefAuthCallback callback)
        {
            bool retVal = false;
            this.DispatchIfRequired(() =>
            {
                try
                {
                    LoginAuthenticationForm authForm = new LoginAuthenticationForm(host);
                    WindowInteropHelper wih = new WindowInteropHelper(authForm);
                    wih.Owner = Handle;
                    var result = authForm.ShowDialog();
                    if (result != null && result.HasValue && result.Value)
                    {
                        callback.Continue(authForm.UserName, authForm.Password);
                        retVal = true;
                    }
                    else
                        callback.Cancel();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in GetAuthCredentials.", ex);
                }
            });
            return retVal;
        }

        #endregion

        /// <summary>
        /// Match browser zoom level to system dpi
        /// Adjust zoom level for system dpi 125%
        /// </summary>
        private void SetBrowserZoomLevel()
        {
            if (_browser != null)
            {
                using (var browserHost = _browser.GetHost())
                {
                    var hdc = IntPtr.Zero;
                    try
                    {
                        hdc = NativeMethods.GetDC(_browserWindowHandle);
                        int ppix = NativeMethods.GetDeviceCaps(hdc, NativeMethods.LOGPIXELSX);
                        if (ppix == 120)
                        {
                            var zoomLevel = ppix/100.0;
                            browserHost.SetZoomLevel(zoomLevel);
                        }
                    }
                    finally
                    {
                        if (hdc != IntPtr.Zero)
                        {
                            NativeMethods.ReleaseDC(_browserWindowHandle, hdc);
                        }
                    }
                }
            }
        }

        private void InvokeHandler<T>(EventHandler<T> handler, T args, bool invokeAsync = false, string callingMethodName = null)
            where T : EventArgs
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.DispatchIfRequired(() =>
            {
                try
                {
                    if (handler != null)
                    {
                        handler(this, args);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in {0} : {1}", callingMethodName ?? "Unknown", ex);
                }
            }, invokeAsync);
        }

        private void InvokeHandler(EventHandler handler, EventArgs args, bool invokeAsync = false, string callingMethodName = null)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.DispatchIfRequired(() =>
            {
                try
                {
                    if (handler != null)
                    {
                        handler(this, args);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in {0} : {1}", callingMethodName ?? "Unknown", ex);
                }
            }, invokeAsync);
        }

        private bool IsResourceAccessible(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (url.StartsWith(DevToolsUrlPrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                var uri = new Uri(url);
                if (!uri.IsAbsoluteUri)
                {
                    return true;
                }

                var host = uri.DnsSafeHost;
                if (string.IsNullOrEmpty(host)
                    || host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase)
                    || host.EndsWith(PackagedApplicationSchemeHandlerFactory.Domain))
                {
                    return true;
                }
            }

            return false;
        }

        private void NavigateTo(string url)
        {
            Logger.Info(string.Format("Current url = {0}, new url = {1}", _currentUrl, url ?? string.Empty));
            if (!string.IsNullOrEmpty(url) && _browser != null && _controlCreated)
            {
                if (!url.Equals(_currentUrl))
                {
                    _currentUrl = url;
                    try
                    {
                        if (_browser.IsLoading)
                        {
                            Logger.Info(string.Format("Stopping the loading of {0}", _currentUrl));
                            _browser.StopLoad();
                        }
                    }
                    finally
                    {
                        Logger.Info(string.Format("Loading url = {0}", _currentUrl));
                        _browser.GetMainFrame().LoadUrl(_currentUrl);
                    }
                }
            }
        }

        protected virtual CefWebBrowser CreatePopupBrowser(int parentId, IBrowserSideMessageRouter router)
        {
            return new CefWebBrowser(parentId, router);
        }

        private ShowPopupEventArgs RaiseShowPopup(CefWebBrowser browser)
        {
            if (ShowPopup != null)
            {
                var ea = new ShowPopupEventArgs(browser);
                ShowPopup(this, ea);
                return ea;
            }
            return null;
        }

        private class DevToolsWebBrowser : BrowserHwndHost, IDisposable
        {
            private CefBrowser _browser;
            private DevToolsWebClient _client;
            public event EventHandler Disposed;

            public CefClient Client
            {
                get { return _client; }
            }

            public DevToolsWebBrowser()
            {
                _client = new DevToolsWebClient(this);
            }

            void IDisposable.Dispose()
            {
                Dispose(true);
            }

            public override IntPtr BrowserWindowHandle
            {
                get
                {
                    return _browser != null ? _browser.GetHost().GetWindowHandle() : IntPtr.Zero;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    if (_client != null)
                    {
                        _client.Dispose();
                        _client = null;
                    }

                    _browser = null;
                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }

            private void OnAfterCreated(CefBrowser browser)
            {
                _browser = browser;
                this.DispatchIfRequired(() => OnHandleCreated());
            }

            private void OnBeforeClose(CefBrowser browser)
            {
                this.DispatchIfRequired(Dispose, true);
            }

            private class DevToolsWebClient : CefClient, IDisposable
            {
                private readonly CefLifeSpanHandler _lifeSpanHandler;
                private DevToolsWebBrowser _ctl;

                public DevToolsWebClient(DevToolsWebBrowser ctl)
                {
                    _ctl = ctl;
                    _lifeSpanHandler = new DefToolsLifeSpanHandler(ctl);
                }

                public void Dispose()
                {
                    _ctl = null;
                }

                protected override CefLifeSpanHandler GetLifeSpanHandler()
                {
                    return _lifeSpanHandler;
                }

                private class DefToolsLifeSpanHandler : CefLifeSpanHandler, IDisposable
                {
                    private DevToolsWebBrowser _ctl;

                    public DefToolsLifeSpanHandler(DevToolsWebBrowser ctl)
                    {
                        _ctl = ctl;
                    }

                    public void Dispose()
                    {
                        _ctl = null;
                    }

                    protected override bool DoClose(CefBrowser browser)
                    {
                        return false;
                    }

                    protected override void OnAfterCreated(CefBrowser browser)
                    {
                        _ctl.OnAfterCreated(browser);
                    }

                    protected override void OnBeforeClose(CefBrowser browser)
                    {
                        _ctl.OnBeforeClose(browser);
                        _ctl = null;
                    }
                }
            }
        }
    }
}