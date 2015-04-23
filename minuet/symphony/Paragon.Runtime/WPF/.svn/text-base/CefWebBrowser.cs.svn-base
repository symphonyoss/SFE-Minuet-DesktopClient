using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Paragon.Plugins;
using Paragon.Runtime.WinForms;
using Xilium.CefGlue;
using Control = System.Windows.Forms.Control;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Paragon.Runtime.WPF
{
    public class CefWebBrowser : ContentControl, ICefWebBrowser
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public CefWebBrowser()
            : this(new WinFormsCefBrowser())
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected CefWebBrowser(WinFormsCefBrowser control)
        {
            KeyboardNavigation.SetIsTabStop(this, false);
            KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.Cycle);

            WinFormsControl = control;
            WinFormsControlHost = new WindowsFormsHost();
            Loaded += OnLoaded;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                WinFormsControlHost.Child = WinFormsControl;
                AttachEventWrappers();
            }

            Content = WinFormsControlHost;
        }

        protected WinFormsCefBrowser WinFormsControl { get; private set; }
        protected WindowsFormsHost WinFormsControlHost { get; private set; }

        public event EventHandler<BrowserCreateEventArgs> BeforeBrowserCreate;
        public event EventHandler<BeforePopupEventArgs> BeforePopup;
        public event EventHandler BrowserAfterCreated;
        public event EventHandler BrowserClosed;
        public event EventHandler<TakeFocusEventArgs> BrowserLostFocus;
        public event EventHandler<ShowPopupEventArgs> ShowPopup;
        public event EventHandler<TitleChangedEventArgs> TitleChanged;
        public event EventHandler<LoadEndEventArgs> LoadEnd;
        public event EventHandler<LoadErrorEventArgs> LoadError;
        public event EventHandler<RenderProcessTerminatedEventArgs> RenderProcessTerminated;
        public event EventHandler<ContextMenuCommandEventArgs> ContextMenuCommand;
        public event EventHandler<ContextMenuEventArgs> BeforeContextMenu;
        public event EventHandler<DownloadProgressEventArgs> DownloadUpdated;
        public event EventHandler<ResourceLoadEventArgs> BeforeResourceLoad;
        public event EventHandler<JsDialogEventArgs> JSDialog;
        public event EventHandler<UnloadDialogEventArgs> BeforeUnloadDialog;
        public event EventHandler<DragEnterEventArgs> WebDragEnter;
        public event EventHandler<ProtocolExecutionEventArgs> ProtocolExecution;

        public string BrowserName
        {
            get { return WinFormsControl.BrowserName; }
        }

        public int Identifier
        {
            get { return WinFormsControl.Identifier; }
        }

        public string Source
        {
            get { return WinFormsControl.Source; }
            set { WinFormsControl.Source = value; }
        }

        public void Reload(bool ignoreCache)
        {
            WinFormsControl.Reload(ignoreCache);
        }

        public void ExecuteJavaScript(string script)
        {
            WinFormsControl.ExecuteJavaScript(script);
        }

        public void Close(bool force = false)
        {
            if (WinFormsControl != null)
            {
                WinFormsControl.Close(force);
            }
        }

        public IDisposable GetDeveloperToolsControl(CefPoint element, CefWindowInfo info = null, CefBrowserSettings settings = null)
        {
            var ctl = WinFormsControl.GetDeveloperToolsControl(element, info, settings);
            return ctl != null ? new DevToolsControl(ctl) : null;
        }

        public void BlurBrowser()
        {
            if (WinFormsControl != null)
            {
                WinFormsControl.BlurBrowser();
            }
        }

        public void FocusBrowser()
        {
            if (WinFormsControl != null)
            {
                WinFormsControl.FocusBrowser();
            }
        }

        public void SetTopMost(bool set = true)
        {
            if (WinFormsControl != null)
            {
                WinFormsControl.SetTopMost(set);
            }
        }

        public void CreateControl()
        {
            WinFormsControl.CreateControl();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (WinFormsControlHost != null)
            {
                WinFormsControlHost.Dispose();
                WinFormsControlHost = null;
            }

            if (WinFormsControl != null)
            {
                DetachEventWrappers();
                WinFormsControl = null;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (WinFormsControl != null)
            {
                WinFormsControl.OnParentLoaded();
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void AttachEventWrappers()
        {
            WinFormsControl.KeyDown += OnControlKeyDown;
            WinFormsControl.PreviewKeyDown += OnControlPreviewKeyDown;
            WinFormsControl.BeforeBrowserCreate += OnControlBeforeBrowserCreate;
            WinFormsControl.BrowserAfterCreated += OnControlBrowserAfterCreated;
            WinFormsControl.BeforePopup += OnControlBrowserBeforePopup;
            WinFormsControl.ShowPopup += OnControlShowPopup;
            WinFormsControl.TitleChanged += OnControlTitleChanged;
            WinFormsControl.LoadEnd += OnControlLoadEnd;
            WinFormsControl.LoadError += OnControlLoadError;
            WinFormsControl.RenderProcessTerminated += OnControlRenderProcessTerminated;
            WinFormsControl.ContextMenuCommand += OnControlContextMenuCommand;
            WinFormsControl.BeforeContextMenu += OnControlBeforeContextMenu;
            WinFormsControl.DownloadUpdated += OnControlDownloadUpdated;
            WinFormsControl.BeforeResourceLoad += OnControlBeforeResourceLoad;
            WinFormsControl.JSDialog += OnControlJSDialog;
            WinFormsControl.BeforeUnloadDialog += OnControlBeforeUnloadDialog;
            WinFormsControl.WebDragEnter += OnWebDragEnter;
            WinFormsControl.BrowserLostFocus += ControlLostFocusEventHandler;
            WinFormsControl.BrowserClosed += OnControlBrowserClosed;
            WinFormsControl.ProtocolExecution += OnProtocolExecution;
        }

        private void DetachEventWrappers()
        {
            WinFormsControl.PreviewKeyDown -= OnControlPreviewKeyDown;
            WinFormsControl.BeforeBrowserCreate -= OnControlBeforeBrowserCreate;
            WinFormsControl.BrowserAfterCreated -= OnControlBrowserAfterCreated;
            WinFormsControl.BeforePopup -= OnControlBrowserBeforePopup;
            WinFormsControl.ShowPopup -= OnControlShowPopup;
            WinFormsControl.TitleChanged -= OnControlTitleChanged;
            WinFormsControl.LoadEnd -= OnControlLoadEnd;
            WinFormsControl.LoadError -= OnControlLoadError;
            WinFormsControl.RenderProcessTerminated -= OnControlRenderProcessTerminated;
            WinFormsControl.ContextMenuCommand -= OnControlContextMenuCommand;
            WinFormsControl.BeforeContextMenu -= OnControlBeforeContextMenu;
            WinFormsControl.DownloadUpdated -= OnControlDownloadUpdated;
            WinFormsControl.BeforeResourceLoad -= OnControlBeforeResourceLoad;
            WinFormsControl.JSDialog -= OnControlJSDialog;
            WinFormsControl.BeforeUnloadDialog -= OnControlBeforeUnloadDialog;
            WinFormsControl.WebDragEnter -= OnWebDragEnter;
            WinFormsControl.BrowserLostFocus -= ControlLostFocusEventHandler;
            WinFormsControl.BrowserClosed -= OnControlBrowserClosed;
            WinFormsControl.ProtocolExecution -= OnProtocolExecution;
        }

        private void OnControlBrowserClosed(object sender, EventArgs e)
        {
            BrowserClosed.Raise(this, e);
        }

        private void OnControlBeforeBrowserCreate(object sender, BrowserCreateEventArgs e)
        {
            BeforeBrowserCreate.Raise(this, e);
        }

        private void OnControlBrowserAfterCreated(object sender, EventArgs e)
        {
            BrowserAfterCreated.Raise(this, e);
        }

        private void OnControlBrowserBeforePopup(object sender, BeforePopupEventArgs e)
        {
            BeforePopup.Raise(this, e);
        }

        protected virtual CefWebBrowser CreatePopupBrowser(WinFormsCefBrowser winFormsBrowser)
        {
            return new CefWebBrowser(winFormsBrowser);
        }

        private void OnControlShowPopup(object source, ShowPopupEventArgs ea)
        {
            var handler = ShowPopup;
            if (handler != null)
            {
                var winFormsBrowser = (WinFormsCefBrowser) ea.PopupBrowser;
                var browser = CreatePopupBrowser(winFormsBrowser);

                browser.RenderSize = new Size(winFormsBrowser.Width, winFormsBrowser.Height);
                browser.Width = browser.RenderSize.Width;
                browser.Height = browser.RenderSize.Height;
                var ea2 = new ShowPopupEventArgs(browser);
                handler(this, ea2);

                ea.Shown = ea2.Shown;
                if (!ea.Shown)
                {
                    browser.WinFormsControl = null;
                    browser.Close();
                }
            }
        }

        private void OnControlTitleChanged(object sender, TitleChangedEventArgs e)
        {
            TitleChanged.Raise(this, e);
        }

        private void OnControlLoadEnd(object sender, LoadEndEventArgs e)
        {
            LoadEnd.Raise(this, e);
        }

        private void OnControlLoadError(object sender, LoadErrorEventArgs e)
        {
            LoadError.Raise(this, e);
        }

        private void OnControlRenderProcessTerminated(object sender, RenderProcessTerminatedEventArgs e)
        {
            RenderProcessTerminated.Raise(this, e);
        }

        private void OnControlContextMenuCommand(object sender, ContextMenuCommandEventArgs e)
        {
            ContextMenuCommand.Raise(this, e);
        }

        private void OnControlBeforeContextMenu(object sender, ContextMenuEventArgs e)
        {
            BeforeContextMenu.Raise(this, e);
        }

        private void OnControlDownloadUpdated(object sender, DownloadProgressEventArgs e)
        {
            DownloadUpdated.Raise(this, e);
        }

        private void OnControlBeforeResourceLoad(object sender, ResourceLoadEventArgs e)
        {
            BeforeResourceLoad.Raise(this, e);
        }

        private void OnControlJSDialog(object sender, JsDialogEventArgs e)
        {
            JSDialog.Raise(this, e);
        }

        private void OnControlBeforeUnloadDialog(object sender, UnloadDialogEventArgs e)
        {
            BeforeUnloadDialog.Raise(this, e);
        }

        private void OnWebDragEnter(object sender, DragEnterEventArgs e)
        {
            WebDragEnter.Raise(this, e);
        }

        private void ControlLostFocusEventHandler(object sender, TakeFocusEventArgs e)
        {
            BrowserLostFocus.Raise(this, e);
        }

        private void OnProtocolExecution(object sender, ProtocolExecutionEventArgs e)
        {
            ProtocolExecution.Raise(this, e);
        }

        private void OnControlKeyDown(object sender, KeyEventArgs e)
        {
            var source = PresentationSource.FromDependencyObject(this);
            if (source == null)
            {
                return;
            }

            var key = KeyInterop.KeyFromVirtualKey((int) e.KeyData);
            var args = new System.Windows.Input.KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) {RoutedEvent = KeyDownEvent};
            RaiseEvent(args);
        }

        private void OnControlPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            var source = PresentationSource.FromDependencyObject(this);
            if (source == null)
            {
                return;
            }

            var key = KeyInterop.KeyFromVirtualKey((int) e.KeyData);
            var args = new System.Windows.Input.KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) {RoutedEvent = PreviewKeyDownEvent};
            RaiseEvent(args);
        }

        private class DevToolsControl : ContentControl, IDisposable
        {
            private readonly WindowsFormsHost _wfHost;
            private IDisposable _ctl;

            public DevToolsControl(IDisposable ctl)
            {
                _ctl = ctl;
                _wfHost = new WindowsFormsHost();
                var control = (Control) _ctl;
                _wfHost.Child = control;
                Content = _wfHost;
                SizeChanged += OnSizeChanged;
            }

            public void Dispose()
            {
                if (_ctl != null)
                {
                    if (_wfHost != null)
                    {
                        _wfHost.Child = null;
                    }
                    _ctl.Dispose();
                    _ctl = null;
                }

                _wfHost.Dispose();
            }

            private void OnSizeChanged(object sender, SizeChangedEventArgs e)
            {
                if (_ctl != null)
                {
                    ((Control) _ctl).Size = new System.Drawing.Size((int) e.NewSize.Width, (int) e.NewSize.Height);
                }
            }
        }
    }
}