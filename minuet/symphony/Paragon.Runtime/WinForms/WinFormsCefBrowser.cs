using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;
using Paragon.Plugins;
using Paragon.Runtime.PackagedApplication;
using Paragon.Runtime.Plugins;
using Paragon.Runtime.Properties;
using Paragon.Runtime.Win32;
using Xilium.CefGlue;

namespace Paragon.Runtime.WinForms
{
    public sealed class WinFormsCefBrowser : Control, ICefWebBrowserInternal, ICefWebBrowser
    {
        private const string DevToolsUrlPrefix = "chrome-devtools://devtools/";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly List<string> _allowedProtocols = new List<string>();

        private readonly int _parentBrowserId = -1;
        private readonly List<string> _whitelistedDomains = new List<string>();
        private CefBrowser _browser;
        private IntPtr _browserWindowHandle;
        private CefWebClient _client;
        private ContainerWindowMoveListener _containerWindowMoveListener;
        private string _currentUrl = "about:blank";
        private bool _disableContextMenu;
        private bool _disablePopups;
        private FocusLossListener _focusLossListener;
        private bool _handleCreated;
        private bool _mainFrameResourceNotAccessible;
        private bool _mainFrameCertificateError;
        private string _name = Guid.NewGuid().ToString();
        private IBrowserSideMessageRouter _router;
        private string _sourceUrl = "about:blank";

        public WinFormsCefBrowser()
        {
            Initialize();
        }

        private WinFormsCefBrowser(int parentId, IBrowserSideMessageRouter router)
        {
            _parentBrowserId = parentId;
            Initialize();
            _router = router;
        }

        private bool IsPopup
        {
            get { return _parentBrowserId > 0 || (_browser != null && _browser.IsPopup); }
        }

        public event EventHandler<BrowserCreateEventArgs> BeforeBrowserCreate;
        public event EventHandler<ContextMenuEventArgs> BeforeContextMenu;
        public event EventHandler<BeforePopupEventArgs> BeforePopup;
        public event EventHandler<ResourceLoadEventArgs> BeforeResourceLoad;
        public event EventHandler<UnloadDialogEventArgs> BeforeUnloadDialog;
        public event EventHandler BrowserAfterCreated;
        public event EventHandler BrowserClosed;
        public event EventHandler<TakeFocusEventArgs> BrowserLostFocus;
        public event EventHandler<ContextMenuCommandEventArgs> ContextMenuCommand;
        public event EventHandler<DownloadProgressEventArgs> DownloadUpdated;
        public event EventHandler<JsDialogEventArgs> JSDialog;
        public event EventHandler<LoadEndEventArgs> LoadEnd;
        public event EventHandler<LoadErrorEventArgs> LoadError;
        public event EventHandler<RenderProcessTerminatedEventArgs> RenderProcessTerminated;
        public event EventHandler<ShowPopupEventArgs> ShowPopup;
        public event EventHandler<TitleChangedEventArgs> TitleChanged;
        public event EventHandler<DragEnterEventArgs> WebDragEnter;
        public event EventHandler<ProtocolExecutionEventArgs> ProtocolExecution;

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
            get { return _sourceUrl; }
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

        public void Close(bool force = false)
        {
            Logger.Info(fmt => fmt("Closing browser {0}", Identifier));
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

        public void OnParentLoaded()
        {
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

                if (info == null)
                {
                    info = CefWindowInfo.Create();
                    info.Width = ClientSize.Width;
                    info.Height = ClientSize.Height;
                }

                info.SetAsChild(ctl.Handle, new CefRectangle(0, 0, info.Width, info.Height));
                ctl.ClientSize = new Size(info.Width, info.Height);
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
                    _browser.GetHost().ShowDevTools(info, ctl.Client, settings);
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
            if (_focusLossListener != null)
            {
                _focusLossListener.SetTopMost(set);
            }
        }

        void ICefWebBrowserInternal.OnBeforeContextMenu(ContextMenuEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            // However, we shall fire this on the IO thread, because the Model is a value type and changes made in the
            // UI thread will be made on a copy

            try
            {
                if (!_disableContextMenu)
                {
                    BeforeContextMenu.Raise(this, ea);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(fmt => fmt("Error in OnBeforeContextMenu : {0}", ex));
            }
            finally
            {
                if (!ea.Handled && _disableContextMenu)
                {
                    ea.Model.Clear();
                }
            }
        }

        void ICefWebBrowserInternal.OnBeforePopup(BeforePopupEventArgs e)
        {
            this.InvokeIfRequired(() =>
            {
                try
                {
                    var beforePopup = BeforePopup;
                    if (!_disablePopups && beforePopup != null)
                    {
                        // If no initial size specified, default to the parent window's size
                        if (e.WindowInfo.Width <= 0)
                        {
                            e.WindowInfo.Width = ClientSize.Width;
                        }
                        if (e.WindowInfo.Height <= 0)
                        {
                            e.WindowInfo.Height = ClientSize.Height;
                        }

                        beforePopup(this, e);

                        if (!e.Cancel && e.NeedCustomPopupWindow)
                        {
                            var name = string.IsNullOrEmpty(e.TargetFrameName)
                                ? Guid.NewGuid().ToString() : e.TargetFrameName;

                            // Create a new WinFormsCefBrowser Control
                            var newBrowser = new WinFormsCefBrowser(Identifier, _router)
                            {
                                Source = e.TargetUrl,
                                _currentUrl = e.TargetUrl,
                                ClientSize = new Size(e.WindowInfo.Width, e.WindowInfo.Height),
                                _name = name
                            };

                            // Force the creation of the control handle
                            newBrowser.CreateControl();
                            // Set the parent and size information for the popup browser control to be created
                            e.WindowInfo.SetAsChild(newBrowser.Handle, new CefRectangle(0, 0, newBrowser.ClientSize.Width, newBrowser.ClientSize.Height));
                            // Set the client to new browser's client so that the OnAfterBrowserCreated (and other notifications) go to that control
                            e.Client = newBrowser._client;
                        }
                    }
                }
                catch (Exception ex)
                {
                    e.Cancel = true;
                    Logger.Error(fmt => fmt("Error in OnBeforePopup : {0}. Popup will not be allowed.", ex));
                }
                finally
                {
                    if (!e.Cancel && _disablePopups)
                    {
                        e.Cancel = true;
                    }
                }
            });
        }

        void ICefWebBrowserInternal.OnCertificateError()
        {
            _mainFrameCertificateError = true;
        }

        void ICefWebBrowserInternal.OnBeforeResourceLoad(ResourceLoadEventArgs ea)
        {
            // Invoke synchronously because the ResourceLoadEventArgs.Cancel property is read
            // by callers to determine whether the resource should be loaded or not.
            this.InvokeIfRequired(() =>
            {
                try
                {
                    if (!IsResourceAccessible(ea.Url))
                    {
                        // Give the container a chance to override
                        BeforeResourceLoad.Raise(this, ea);

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
                    Logger.Error(fmt => fmt("Error in OnBeforeResourceLoad : {0}. Resource loading will be aborted.", ex));
                    ExecuteJavaScript("window.console.error('Error loading resource: " + ea.Url + "');");
                    ea.Cancel = true;
                }
            });
        }

        void ICefWebBrowserInternal.OnBeforeUnloadDialog(UnloadDialogEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.InvokeIfRequired(() =>
            {
                try
                {
                    BeforeUnloadDialog.Raise(this, ea);
                    if (!ea.Handled)
                    {
                        // Show the dialogs asynchronously
                        BeginInvoke(new Action(() =>
                        {
                            var r = MessageBox.Show(ea.MessageText, "JavaScript", MessageBoxButtons.OKCancel);

                            if (_browser != null)
                            {
                                ea.Callback.Continue(r == DialogResult.OK, string.Empty);
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(fmt => fmt("Error in OnBeforeUnloadDialog : {0}. Dialog will be suppressed.", ex));
                }
                finally
                {
                    // Tell CEF we handled the dialog
                    ea.Handled = true;
                }
            });
        }

        void ICefWebBrowserInternal.OnBrowserAfterCreated(CefBrowser browser)
        {
            this.InvokeIfRequired(() =>
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
                            _focusLossListener = new FocusLossListener(_browserWindowHandle);
                        }
                    }
                    ResizeWindow(_browserWindowHandle, Width, Height);
                    if (!IsPopup)
                    {
                        BrowserAfterCreated.Raise(this, EventArgs.Empty);
                    }
                    else
                    {
                        var parent = ParagonRuntime.FindBrowser<WinFormsCefBrowser>(_parentBrowserId);
                        if (parent == null)
                        {
                            Close();
                        }
                        else
                        {
                            var ea = parent.RaiseShowPopup(this);
                            if (!ea.Shown)
                            {
                                Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(fmt => fmt("Error in OnBrowserAfterCreated : {0}", ex));
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
                Logger.Error(fmt => fmt("Error in OnBeforeContextMenu : {0}", ex));
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

        void ICefWebBrowserInternal.OnDragEnter(DragEnterEventArgs args)
        {
            InvokeHandler(WebDragEnter, args);
        }

        void ICefWebBrowserInternal.OnJSDialog(JsDialogEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.InvokeIfRequired(() =>
            {
                try
                {
                    JSDialog.Raise(this, ea);
                    if (!ea.Handled)
                    {
                        // Show the dialogs asynchronously
                        BeginInvoke(new Action(() =>
                        {
                            var owner = new Win32Window(Handle);
                            var dialogResult = DialogResult.OK;
                            var msg = string.Empty;
                            switch (ea.DialogType)
                            {
                                case CefJSDialogType.Alert:
                                    MessageBox.Show(owner, ea.MessageText, "JavaScript Alert", MessageBoxButtons.OK);
                                    ea.Handled = true;
                                    break;
                                case CefJSDialogType.Confirm:
                                    dialogResult = MessageBox.Show(owner, ea.MessageText, "JavaScript Prompt", MessageBoxButtons.OKCancel);
                                    break;
                                case CefJSDialogType.Prompt:
                                    msg = ea.DefaultPromptText;
                                    dialogResult = PromptDialog.Prompt(owner, ea.MessageText, ref msg);
                                    break;
                            }
                            if (_browser != null)
                            {
                                ea.Callback.Continue(dialogResult == DialogResult.OK, msg);
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(fmt => fmt("Error in OnJSDialog : {0}. Dialog will be suppressed.", ex));
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

            this.InvokeIfRequired(() =>
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
                    Logger.Error(fmt => fmt("Error in OnLoadEnd : {0}", ex));
                }
                finally
                {
                    LoadEnd.Raise(this, e);
                }
            },
                true);
        }

        void ICefWebBrowserInternal.OnLoadError(LoadErrorEventArgs e)
        {
            // OnLoadError is called when an error occurs while loading the main frame 
            // resource (resource of type CefResourceType.MainFrame). OnLoadError is not
            // called when child resources of the main frame (scripts, etc) fail to load.
            this.InvokeIfRequired(() =>
            {
                try
                {
                    if (_mainFrameCertificateError)
                    {
                        var html = StaticWebContent.GetInvalidCertErrorPage(e.FailedUrl);
                        e.Frame.LoadString(html, PackagedApplicationSchemeHandlerFactory.ErrorPage);
                        Logger.Error("Unable to load the main frame due to an invalid certifiate: " + e.FailedUrl);
                        LoadError.Raise(this, e);
                    }
                    else if (_mainFrameResourceNotAccessible)
                    {
                        // The resource associated with the main frame is not accessible.
                        // The domain may not be whitelisted, etc.
                        var html = StaticWebContent.GetResourceNotAccessibleErrorPage(e.FailedUrl);
                        e.Frame.LoadString(html, PackagedApplicationSchemeHandlerFactory.ErrorPage);
                        Logger.Error("Main frame resource not accessible: " + e.FailedUrl);
                        LoadError.Raise(this, e);
                    }
                    else if (e.ErrorCode != CefErrorCode.Aborted)
                    {
                        // The main frame failed to load for some reason. Show the default error
                        // page along with any error text that may have been provided.
                        var html = StaticWebContent.GetLoadErrorPage(e.FailedUrl, e.ErrorText, ParagonLogManager.CurrentParagonLogFile);
                        e.Frame.LoadString(html, PackagedApplicationSchemeHandlerFactory.ErrorPage);
                        Logger.Error(fmt => fmt("Error loading the main frame: url = {0}, Error = {1} {2}", e.FailedUrl, e.ErrorCode, e.ErrorText));
                        LoadError.Raise(this, e);
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
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception encountered while handling a page load error", ex);
                }
            }, true);
        }

        void ICefWebBrowserInternal.OnLoadStart(LoadStartEventArgs e)
        {
            this.InvokeIfRequired(() =>
            {
                try
                {
                    ExecuteJavaScript(StaticWebContent.GetOnLoadStartScript());
                }
                catch (Exception ex)
                {
                    Logger.Error(fmt => fmt("Error in OnLoadStart : {0}", ex));
                }
            },
            true);
        }

        void ICefWebBrowserInternal.OnPreviewKeyEvent(CefKeyEvent keyEvent)
        {
            this.InvokeIfRequired(() =>
            {
                if (keyEvent.EventType == CefKeyEventType.RawKeyDown)
                {
                    var keys = (Keys) keyEvent.WindowsKeyCode;
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
                    var args = new PreviewKeyDownEventArgs(keys);
                    base.OnPreviewKeyDown(args);
                }
            });
        }

        bool ICefWebBrowserInternal.OnProcessMessageReceived(CefBrowser browser, CefProcessMessage message)
        {
            return _router.ProcessCefMessage(browser, message);
        }

        void ICefWebBrowserInternal.OnRenderProcessTerminated(RenderProcessTerminatedEventArgs e)
        {
            InvokeHandler(RenderProcessTerminated, e, true);
        }

        void ICefWebBrowserInternal.OnTakeFocus(TakeFocusEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            InvokeHandler(BrowserLostFocus, ea);
        }

        void ICefWebBrowserInternal.OnTitleChanged(TitleChangedEventArgs e)
        {
            InvokeHandler(TitleChanged, e, true);
        }

        void ICefWebBrowserInternal.OnProtocolExecution(ProtocolExecutionEventArgs ea)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.InvokeIfRequired(() =>
            {
                try
                {
                    ProtocolExecution.Raise(this, ea);
                    if (ea.Allow)
                    {
                        var scheme = new Uri(ea.Url).Scheme;
                        if (!_allowedProtocols.Contains(scheme))
                            _allowedProtocols.Add(scheme);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(fmt => fmt("Error in OnProtocolExecution : {0}. Protocol will not be allowed.", ex.Message));
                    ea.Allow = false;
                }
            });
        }

        bool ICefWebBrowserInternal.OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool isRedirect)
        {
            var scheme = new Uri(request.Url).Scheme;
            return !string.IsNullOrEmpty(scheme) && !CefBrowserApplication.AllowedProtocols.Contains(scheme);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void Dispose(bool disposing)
        {
            if (_browser != null && disposing)
            {
                ParagonRuntime.RemoveBrowser(this);
                if (_focusLossListener != null)
                {
                    _focusLossListener.Dispose();
                }
                if (_containerWindowMoveListener != null)
                {
                    _containerWindowMoveListener.Dispose();
                }
                if (_router != null)
                {
                    _router.Dispose();
                }
                if (_client != null)
                {
                    _client.Dispose();
                }
                if (_browser != null)
                {
                    _browser.Dispose();
                }
                _browser = null;
                _browserWindowHandle = IntPtr.Zero;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!_handleCreated && !DesignMode)
            {
                if (!ParagonRuntime.IsInitialized)
                {
                    ParagonRuntime.Initialize();
                }

                _client = new CefWebClient(this);

                if (!IsPopup && _browser == null)
                {
                    var settings = new CefBrowserSettings
                    {
                        Java = CefState.Disabled
                    };

                    using (AutoStopwatch.TimeIt("Creating browser"))
                    {
                        var info = CefWindowInfo.Create();
                        var ea = new BrowserCreateEventArgs();

                        using (AutoStopwatch.TimeIt("Raising BeforeBrowserCreate event"))
                        {
                            BeforeBrowserCreate.Raise(this, ea);
                        }

                        _router = ea.Router ?? new BrowserSideMessageRouter();
                        info.SetAsChild(Handle, new CefRectangle(0, 0, Width, Height));
                        _currentUrl = _sourceUrl;
                        Logger.Info(fmt => fmt("OnHandleCreated - Creating a browser with url {0}", _currentUrl));
                        CefBrowserHost.CreateBrowser(info, _client, settings, _currentUrl);
                    }
                }

                _handleCreated = true;
            }
        }

        protected override void OnResize(EventArgs args)
        {
            base.OnResize(args);
            if (_browserWindowHandle != IntPtr.Zero)
            {
                // Ignore size changes when form are minimized.
                var form = TopLevelControl as Form;
                if (form != null && form.WindowState == FormWindowState.Minimized)
                {
                    return;
                }
                ResizeWindow(_browserWindowHandle, Width, Height);
            }
        }

        private void Initialize()
        {
            SetStyle(
                ControlStyles.ContainerControl
                | ControlStyles.ResizeRedraw
                | ControlStyles.FixedWidth
                | ControlStyles.FixedHeight
                | ControlStyles.StandardClick
                | ControlStyles.UserMouse
                | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.StandardDoubleClick
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.CacheText
                | ControlStyles.EnableNotifyMessage
                | ControlStyles.DoubleBuffer
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UseTextForAccessibility
                | ControlStyles.Opaque,
                false);

            SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.Selectable,
                true);
            _disablePopups = false;
            _disableContextMenu = false;
            Source = "about:blank";
            ParagonRuntime.AddBrowser(this);
            _allowedProtocols.AddRange(CefBrowserApplication.AllowedProtocols);
            foreach (var domain in Settings.Default.WhitelistedDomains)
            {
                _whitelistedDomains.Add(domain);
            }
        }

        private void InvokeHandler<T>(EventHandler<T> handler, T args, bool invokeAsync = false, string callingMethodName = null)
            where T : EventArgs
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.InvokeIfRequired(() =>
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
                    Logger.Error(fmt => fmt("Error in {0} : {1}", callingMethodName ?? "Unknown", ex));
                }
            }, invokeAsync);
        }

        private void InvokeHandler(EventHandler handler, EventArgs args, bool invokeAsync = false, string callingMethodName = null)
        {
            // Raised on UI thread. Invoke is needed if multi-threaded-message-loop is used.
            this.InvokeIfRequired(() =>
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
                    Logger.Error(fmt => fmt("Error in {0} : {1}", callingMethodName ?? "Unknown", ex));
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
                    || _whitelistedDomains.Any(d => host.EndsWith(d, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private void NavigateTo(string url)
        {
            Logger.Info(string.Format("Current url = {0}, new url = {1}", _currentUrl, url ?? string.Empty));
            if (!string.IsNullOrEmpty(url) && _browser != null && _handleCreated)
            {
                if (!url.Equals(_currentUrl))
                {
                    _currentUrl = url;
                    try
                    {
                        if (_browser.IsLoading)
                        {
                            Logger.Info(fmt => fmt("Stopping the loading of {0}", _currentUrl));
                            _browser.StopLoad();
                        }
                    }
                    finally
                    {
                        Logger.Info(fmt => fmt("Loading url = {0}", _currentUrl));
                        _browser.GetMainFrame().LoadUrl(_currentUrl);
                    }
                }
            }
        }

        private ShowPopupEventArgs RaiseShowPopup(WinFormsCefBrowser browser)
        {
            var ea = new ShowPopupEventArgs(browser);
            ShowPopup.Raise(this, ea);
            return ea;
        }

        private static void ResizeWindow(IntPtr handle, int width, int height)
        {
            if (handle != IntPtr.Zero)
            {
                Win32Api.SetWindowPosition(handle, IntPtr.Zero,
                    0, 0, width, height, SWP.NOZORDER | SWP.NOACTIVATE);
            }
        }
    }
}