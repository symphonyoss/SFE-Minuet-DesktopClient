using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
using Paragon.Runtime.Desktop;
using Paragon.Runtime.Kernel.HotKeys;
using Paragon.Runtime.Win32;
using Paragon.Runtime.WinForms;
using Paragon.Runtime.WPF;
using Xilium.CefGlue;
using Button = System.Windows.Controls.Button;
using DownloadProgressEventArgs = Paragon.Plugins.DownloadProgressEventArgs;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Timer = System.Threading.Timer;

namespace Paragon.Runtime.Kernel.Windowing
{
    [JavaScriptPlugin(CallbackThread = CallbackThread.Main)]
    public class ApplicationWindow : ParagonWindow, IApplicationWindowEx, IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private static int _idSeed;
        private ICefWebBrowser _browser;
        private HotKeyService _hotKeyService;
        private string _id;
        private string _appId;
        private bool _isClosed;
        private bool _isClosing;
        private bool _minimizeOnClose;
        private CreateWindowOptions _options;
        private string _title;
        private DeveloperToolsWindow _tools;
        private IApplicationWindowManagerEx _windowManager;
        private JavaScriptPluginCallback _closeHandler;
        private AutoSaveWindowPositionBehavior _autoSaveWindowPositionBehavior;

        public ApplicationWindow()
        {
            Loaded += OnLoaded;
            var nativeWindow = new NativeApplicationWindow(this);
            nativeWindow.AddHook(WndProc);
        }

        public event EventHandler LoadComplete;
        public event EventHandler<DownloadProgressEventArgs> DownloadProgress;
        public event EventHandler<BeginDownloadEventArgs> BeginDownload;

        /// <summary>
        /// Fired when the window is resized.
        /// No parameters required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onBoundsChanged")]
        public event JavaScriptPluginCallback WindowBoundsChanged;

        /// <summary>
        /// Fired when the window is closed.
        /// Note, this should be listened to from a window other than the window being closed, for example from the background
        /// page. This is because the window being closed will be in the process of being torn down when the event is fired, which means
        /// not all APIs in the window's script context will be functional.
        /// No parameters required.
        /// </summary>
        [JavaScriptDispose]
        [JavaScriptPluginMember(Name = "onClosed")]
        public event JavaScriptPluginCallback WindowClosed;

        /// <summary>
        /// Fired when the window is fullscreened.
        /// No parameters required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onFullScreened")]
        public event JavaScriptPluginCallback WindowFullScreened;

        /// <summary>
        /// Fired when the window is maximized.
        /// No parameters required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onMaximized")]
        public event JavaScriptPluginCallback WindowMaximized;

        /// <summary>
        /// Fired when the window is minimized.
        /// No parameters required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onMinimized")]
        public event JavaScriptPluginCallback WindowMinimized;

        /// <summary>
        /// Fired when the window is restored from being minimized or maximized.
        /// No parameters required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onRestored")]
        public event JavaScriptPluginCallback WindowRestored;

        /// <summary>
        /// Fired when the browser completes loading a new page.
        /// </summary>
        [JavaScriptPluginMember(Name = "onPageLoaded")]
        public event JavaScriptPluginCallback PageLoaded;

        /// <summary>
        /// Clear attention to the window.
        /// </summary>
        [JavaScriptPluginMember]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void ClearAttention()
        {
            Flash(true);
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        [JavaScriptPluginMember(Name = "close")]
        public void CloseWindow()
        {
            _minimizeOnClose = false;
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(Close, true); 
        }

        /// <summary>
        /// Called by the web application when it wants to take responsibility for closing the window.
        /// When the user attempts to close the window, the closeHandler callback will be invoked. It
        /// is then up to the application to decide whether to close the window or not. It should close
        /// the window using the window.close() method.
        /// </summary>
        /// <param name="closeHandler"></param>
        [JavaScriptPluginMember, UsedImplicitly]
        public void AssumeWindowCloseAuthority(JavaScriptPluginCallback closeHandler)
        {
            _closeHandler = closeHandler;
        }

        /// <summary>
        /// Called by the web application when it wishes to return responsibility for closing the
        /// window to the native runtime. The closeHandler will no longer be called and the window
        /// will just close. The application may still be notified of the closure via the 
        /// windowClosed event.
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public void RescindWindowCloseAuthority()
        {
            _closeHandler = null;
        }
       
        public bool ContainsBrowser(int browserId)
        {
            return _browser != null && _browser.Identifier == browserId;
        }

        /// <summary>
        /// Draw attention to the window.
        /// </summary>
        [JavaScriptPluginMember]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void DrawAttention(bool autoclear)
        {
            DispatchIfRequired(() => Flash(false, autoclear), true);
        }

        /// <summary>
        /// Focus the window.
        /// </summary>
        [JavaScriptPluginMember(Name = "focus")]
        public void FocusWindow()
        {
            DispatchIfRequired(() => Focus(), true);
        }

        /// <summary>
        /// Bring window to the front.
        /// </summary>
        [JavaScriptPluginMember(Name = "bringToFront")]
        public void BringToFront()
        {
            DispatchIfRequired(() =>
                                   {
                                       Win32Api.SetWindowPosition(Handle, new IntPtr(-1), 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE);
                                       Win32Api.SetWindowPosition(Handle, new IntPtr(-2), 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE);
                                   }, true);
        }

        /// <summary>
        /// Fullscreens the window.
        /// The user will be able to restore the window by pressing ESC.
        /// An application can prevent the fullscreen state to be left when ESC is pressed by requesting the
        /// overrideEscFullscreen
        /// permission and canceling the event by calling .preventDefault(), like this:
        /// window.onKeyDown = function(e) { if (e.keyCode == 27 /* ESC */) { e.preventDefault(); } };
        /// </summary>
        [JavaScriptPluginMember(Name = "fullscreen")]
        public void FullScreenWindow()
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(() =>
            {
                if (IsFullScreenEnabled)
                {
                    return;
                }

                IsFullScreenEnabled = true;
                WindowFullScreened.Raise();
            }, true);
        }

        /// <summary>
        /// The id to uniquely identify the window instance within the application.
        /// </summary>
        /// <returns></returns>
        [JavaScriptPluginMember]
        public string GetId()
        {
            // Unless specified in the CreateWindowOptions object, we generate a window ID based on the
            // application instance ID. Everytime the app creates a window, we increment the seed to
            // provide a unique ID. It's by no means perfect, but it's good enough for now.
            if (string.IsNullOrEmpty(_id))
            {
                if (_options != null && !string.IsNullOrEmpty(_options.Id))
                {
                    _id = _options.Id;
                }
                else
                {
                    _id = _windowManager.Application.Metadata.InstanceId + "_win" + (++_idSeed);
                }
            }

            return _id;
        }

        public string GetAppId()
        {
            //Gets the appID for the paragon application
            if (string.IsNullOrEmpty(_appId))
            {
                if (_windowManager.Application.Metadata.Id != null)
                {
                    _appId = _windowManager.Application.Metadata.Id;
                }
            }

            return _appId;
        }

        /// <summary>
        /// The id to uniquely identify the window instance within the application.
        /// </summary>
        /// <returns></returns>
        [JavaScriptPluginMember]
        public string GetTitle()
        {
            return Dispatcher.Invoke(new Func<string>(() => Title)) as string;
        }

        /// <summary>
        /// The position, size and constraints of the window's content, which does not include window decorations.
        /// FOR REFERENCE: in Chrome Apps, the returned type is Bounds, which includes various functions for setting the size
        /// and position.
        /// </summary>
        /// <returns></returns>
        [JavaScriptPluginMember]
        public BoundsSpecification GetInnerBounds()
        {
            return DispatchIfRequired(() =>
            {
                var r = (FrameworkElement)Content;
                return new BoundsSpecification
                {
                    Left = 0,
                    Top = 0,
                    Height = (int)r.ActualHeight,
                    Width = (int)r.ActualWidth,
                    MaxHeight = (int)(MaxHeight - (Height - r.ActualHeight)),
                    MaxWidth = (int)(MaxWidth - (Width - r.ActualWidth)),
                    MinHeight = (int)(MinHeight - (Height - r.Height)),
                    MinWidth = (int)(MinWidth - (Width - r.Width)),
                };
            }) as BoundsSpecification;
        }

        /// <summary>
        /// The position, size and constraints of the window, which includes window decorations, such as the title bar and frame.
        /// FOR REFERENCE: in Chrome Apps, the returned type is Bounds, which includes various functions for setting the size and position.
        /// </summary>
        /// <returns></returns>
        [JavaScriptPluginMember]
        public BoundsSpecification GetOuterBounds()
        {
            return DispatchIfRequired(GetBounds) as BoundsSpecification;
        }

        /// <summary>
        /// Hide the window. Does nothing if the window is already hidden.
        /// </summary>
        [JavaScriptPluginMember(Name = "hide")]
        public void HideWindow()
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(Hide, true);
        }

        [JavaScriptPluginMember(Name = "setZoomLevel")]
        public void SetZoomLevel(double level)
        {
            DispatchIfRequired(() => { if( _browser != null ) _browser.SetZoomLevel(level); }, true);
        }

        [JavaScriptPluginMember(Name = "runFileDialog")]
        public void RunFileDialog(FileDialogMode mode, string title, string defaultFileName, string[] acceptTypes, JavaScriptPluginCallback callback)
        {
            DispatchIfRequired(() => 
            {
                var cb = new FileDialogCallback(callback);
                var dialogMode = (CefFileDialogMode)Enum.Parse(typeof(CefFileDialogMode), mode.ToString(), true);
                _browser.RunFileDialog(dialogMode, title, defaultFileName, acceptTypes, 0, cb);
            });
        }

        public void Initialize(IApplicationWindowManagerEx windowManager, ICefWebBrowser browser,
            string startUrl, string title, CreateWindowOptions options)
        {
            _windowManager = windowManager;
            _browser = browser;
            _title = title;
            _options = options;

            AttachToBrowser();
            var control = browser as FrameworkElement;

            if (control != null)
            {
                control.Width = double.NaN;
                control.Height = double.NaN;
                control.HorizontalAlignment = HorizontalAlignment.Stretch;
                control.VerticalAlignment = VerticalAlignment.Stretch;
                control.PreviewKeyDown += OnBrowserPreviewKeyDown;
                browser.LoadEnd += OnPageLoaded;
            }

            ApplyWindowOptions();
        }

        /// <summary>
        /// Maximize the window.
        /// </summary>
        [JavaScriptPluginMember]
        public void Maximize()
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(() =>
            {
                if (WindowState != WindowState.Maximized)
                {
                    IsFullScreenEnabled = false;
                    WindowState = WindowState.Maximized;
                }
            }, true);
        }

        /// <summary>
        /// Minimize the window.
        /// </summary>
        [JavaScriptPluginMember]
        public void Minimize()
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(() =>
            {
                if (WindowState != WindowState.Minimized)
                {
                    IsFullScreenEnabled = false;
                    WindowState = WindowState.Minimized;
                }
            }, true);
        }

        /// <summary>
        /// Move the window to the position (|left|, |top|).
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        [JavaScriptPluginMember]
        public void MoveTo(int left, int top)
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(() =>
            {
                Left = left;
                Top = top;
            }, true);
        }

        /// <summary>
        /// Resize the window to |width|x|height| pixels in size.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        [JavaScriptPluginMember]
        public void ResizeTo(int width, int height)
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(() =>
            {
                Width = width;
                Height = height;
            }, true);
        }

        /// <summary>
        /// Restore the window, exiting a maximized, minimized, or fullscreen state.
        /// </summary>
        [JavaScriptPluginMember]
        public void Restore()
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(() =>
            {
                if (IsFullScreenEnabled)
                {
                    IsFullScreenEnabled = false;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            }, true);
        }

        [JavaScriptPluginMember]
        public void SetOuterBounds(BoundsSpecification bounds)
        {
            DispatchIfRequired(() =>
            {
                int top = (int)bounds.Top,
                    left = (int)bounds.Left,
                    width = bounds.Width > 0 ? (int)bounds.Width : (int)Width,
                    height = bounds.Height > 0 ? (int)bounds.Height : (int)Height;

                var virtualSize = new Vector(width, height);
                var realSize = GetRealSize(virtualSize);

                Win32Api.SetWindowPosition(Handle, IntPtr.Zero, left, top, 
                    (int)realSize.Width, (int)realSize.Height, SWP.NOACTIVATE | SWP.NOZORDER);           

                if (bounds.MinHeight > 0)
                {
                    MinHeight = bounds.MinHeight;
                }
                if (bounds.MinWidth > 0)
                {
                    MinWidth = bounds.MinWidth;
                }
                if (bounds.MaxHeight > 0)
                {
                    MaxHeight = bounds.MaxHeight;
                }
                if (bounds.MaxWidth > 0)
                {
                    MaxWidth = bounds.MaxWidth;
                }
            }, true);
        }

        /// <summary>
        /// Show the window. Does nothing if the window is already visible. Focus the window if |focused| is set to true or omitted.
        /// </summary>
        [JavaScriptPluginMember(Name = "show")]
        public void ShowWindow(bool focused = true)
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            DispatchIfRequired(() =>
            {
                if (!IsVisible)
                {
                    Show();
                }
                else if (!focused && !Topmost) // Window already visible
                {
                    if(WindowState == System.Windows.WindowState.Maximized) 
                        Win32Api.ShowWindow(Handle, SW.SHOWNA);
                    else Win32Api.ShowWindow(Handle, SW.SHOWNOACTIVATE);
                    Win32Api.ActivateWindowNoFocus(Handle);
                }

                if (focused)
                {
                    Focus();
                }
            }, true);
        }

        [JavaScriptPluginMember(Name = "refresh")]
        public void RefreshWindow(bool ignoreCache = true)
        {
            DispatchIfRequired(() => _browser.Reload(ignoreCache), true);
        }

        [JavaScriptPluginMember(Name = "executeJavaScript")]
        public void ExecuteJavaScript(string script)
        {
            DispatchIfRequired(() => _browser.ExecuteJavaScript(script), true);
        }

        public IntPtr Handle
        {
            get { return new WindowInteropHelper(this).EnsureHandle(); }
        }

        public ICefWebBrowser Browser
        {
            get { return _browser; }
        }

        public void ShowDeveloperTools(Point element)
        {
            DispatchIfRequired(() =>
            {
                if (_tools == null)
                {
                    var c = _browser.GetDeveloperToolsControl(new CefPoint((int)element.X, (int)element.Y), null, null);
                    _tools = new DeveloperToolsWindow();
                    _tools.Initialize(c, Title, (_options == null || _options.Frame == null || _options.Frame.Type == FrameType.None) ? FrameType.Paragon : _options.Frame.Type);
                    _tools.Closed += ToolsClosed;
                    _tools.Show();
                }
                else
                {
                    _tools.Activate();
                }
            }, true);
        }

        /// <summary>
        /// Fired when a property is change.
        /// property name and new value is required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onPropertyChanged"), UsedImplicitly]
        public event JavaScriptPluginCallback PropertyChanged;

        /// <summary>
        /// Fired when the window receives a registered hotkey.
        /// No parameters required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onHotKeyPressed"), UsedImplicitly]
        public event JavaScriptPluginCallback HotKeyPressed;

        /// <summary>
        /// Fired when the window system menu item was clicked
        /// No parameters required.
        /// </summary>
        [JavaScriptPluginMember(Name = "onSystemMenuItemClicked"), UsedImplicitly]
        public event JavaScriptPluginCallback SystemMenuItemClicked;

        [JavaScriptPluginMember, UsedImplicitly]
        public void Dock(string edge, bool autoHide)
        {
            // TODO: Implement this.
        }

        /// <summary>
        /// When the window gets keyboard focus, transfer the focus to the browser. This does not need to be implemented on the Embedded ApplicationWindow
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (_browser != null)
            {
                _browser.FocusBrowser();
            }
            base.OnGotKeyboardFocus(e);
        }

        /// <summary>
        /// Set focus to this window when activated. This does not need to be implemented on the Embedded ApplicationWindow
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            Keyboard.Focus(this);
            base.OnActivated(e);
        }

        /// <summary>
        /// The JavaScript 'window' object for the created child.
        /// </summary>
        [UsedImplicitly]
        public object GetContentWindow()
        {
            // Should be a property, needs plugin model to support dynamic plugins returning the global cef object from the CefV8Context that created the window.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Is the window always on top?
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public bool IsAlwaysOnTop()
        {
            // Dispatcher.Invoke(Func<bool>) uses a path that works out whether it is already on the right thread or not
            var isAlwaysOnTop = false;
            Dispatcher.Invoke(new Action(() => isAlwaysOnTop = Topmost));
            return isAlwaysOnTop;
        }

        /// <summary>
        /// Is the window fullscreen?
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public bool IsFullScreen()
        {
            // Dispatcher.Invoke(Func<bool>) uses a path that works out whether it is already on the right thread or not
            var isFullScreen = false;
            Dispatcher.Invoke(new Action(() => isFullScreen = IsFullScreenEnabled));
            return isFullScreen;
        }

        /// <summary>
        /// Is the window maximized?
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public bool IsMaximized()
        {
            // Dispatcher.Invoke(Func<bool>) uses a path that works out whether it is already on the right thread or not
            var isMaximized = false;
            Dispatcher.Invoke(new Action(() => isMaximized = WindowState == WindowState.Maximized));
            return isMaximized;
        }

        /// <summary>
        /// Is the window minimized?
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public bool IsMinimized()
        {
            // Dispatcher.Invoke(Func<bool>) uses a path that works out whether it is already on the right thread or not
            var isMinimized = false;
            Dispatcher.Invoke(new Action(() => isMinimized = WindowState == WindowState.Minimized));
            return isMinimized;
        }

        /// <summary>
        /// Set whether the window should stay above most other windows. Requires the "alwaysOnTopWindows" permission.
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public void SetAlwaysOnTop(bool alwaysOnTop)
        {
            // Dispatcher.Invoke(Action) uses a path that works out whether it is already on the right thread or not
            Dispatcher.Invoke(new Action(() =>
            {
                Topmost = alwaysOnTop;
                _browser.SetTopMost(alwaysOnTop);
            }));
        }

        /// <summary>
        /// Set whether window should minimize when the close button is pressed
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public void SetMinimizeOnClose(bool minimizeOnClose)
        {
            _minimizeOnClose = minimizeOnClose;
        }

        [JavaScriptPluginMember(Name = "setHotKeys"), UsedImplicitly]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void SetHotKeys(string name, string modifiers, string keys)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrEmpty(modifiers))
            {
                throw new ArgumentNullException("modifiers");
            }

            if (string.IsNullOrEmpty(keys))
            {
                throw new ArgumentNullException("keys");
            }

            // The JSON serializer converts flags enum values to a comma delimited list. In 
            // order to use the ModifierKeysConverter, we need ot replace the comma with a plus.
            var modifierString = modifiers;
            if (modifierString.Contains(","))
            {
                modifierString = modifierString.Replace(',', '+').Replace(" ", string.Empty);
            }

            var converter = new ModifierKeysConverter();
            var value = converter.ConvertFromString(modifierString);
            if (value == null)
            {
                throw new ArgumentException("Invalid modifier key(s)", "modifiers");
            }

            var modifierKeys = (ModifierKeys)value;
            var key = (Keys)Enum.Parse(typeof(Keys), keys);

            _hotKeyService.Remove(name);
            _hotKeyService.Add(name, modifierKeys, key);
        }

        [JavaScriptPluginMember(Name = "setHotKeysEnabled"), UsedImplicitly]
        public void SetHotKeysEnabled(bool enabled)
        {
            _hotKeyService.IsEnabled = enabled;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void StartDrag()
        {
            Dispatcher.Invoke(new Action(StartDragMove));
        }

        protected override void OnClosed(EventArgs e)
        {
            DoCleanup();
            base.OnClosed(e);
            WindowClosed.Raise(() => new object[] { this });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!_isClosing)
            {
                var closeHandler = _closeHandler;
                if (closeHandler != null)
                {
                    closeHandler.Raise(() => new object[] {});
                }
                else
                {
                    _isClosing = true;
                    CloseCefBrowser();
                }
            }

            if (!_isClosed && Content != null)
            {
                e.Cancel = true;
            }
        }

        private void CloseCefBrowser()
        {
            if (_browser != null)
            {
                if (_tools != null)
                {
                    _tools.Close();
                }

                _browser.Close();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsFullScreenEnabled && e.Key == Key.Escape)
            {
                Restore();
            }
            base.OnKeyDown(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            HandleKeyPress(e.Key);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (_options != null && _options.Frame != null && _options.Frame.SystemMenu != null)
            {
                var interceptor = new SystemMenuInterceptor(_options.Frame.SystemMenu);
                interceptor.ApplyTo(this);
                interceptor.ItemClicked += (sender, args) => SystemMenuItemClicked.Raise(() => new object[] { args.Id, args.IsChecked });
            }

            _hotKeyService = new HotKeyService(this);
            _hotKeyService.HotKeyPressed += (sender, args) => HotKeyPressed.Raise(() => new object[] { args.Name });
            if (_options != null)
            {
                _hotKeyService.IsEnabled = _options.HotKeysEnabled;
            }

            if (_windowManager != null)
            {
                var hwnd = Handle;
                var appId = _windowManager.Application.Metadata.Id;
                var instanceId = _windowManager.Application.Metadata.InstanceId;
                ParagonDesktop.RegisterAppWindow(hwnd, appId, _windowManager.Application.Package.Manifest.ProcessGroup, instanceId);

                if (_options != null && 
                    !string.IsNullOrEmpty(_options.Id) && 
                    _options.AutoSaveLocation)
                {
                    _autoSaveWindowPositionBehavior = new AutoSaveWindowPositionBehavior();
                    _autoSaveWindowPositionBehavior.Attach(this);
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            // Note that _browser is not set in the case of a dev tools window.
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowMaximized.Raise();

                    if (_browser != null)
                    {
                        _browser.FocusBrowser();
                    }
                    break;

                case WindowState.Minimized:
                    WindowMinimized.Raise();
                    break;

                case WindowState.Normal:
                    WindowRestored.Raise();

                    if (_browser != null)
                    {
                        _browser.FocusBrowser();
                    }
                    break;
            }
        }

        private void ApplyFrameStyle(FrameType frameType)
        {
            switch (frameType)
            {
                case FrameType.NotSpecified:
                case FrameType.Paragon:
                    CustomChromeEnabled = WindowsVersion.IsWin7OrNewer;
                    break;

                case FrameType.None:
                    // Hide the window chrome.
                    TitlebarHeight = 0;
                    WindowStyle = WindowStyle.None;
#if ENFORCE_PACKAGE_SECURITY
                    if (_windowManager.Application.Package.Signature == null)
                    {
                        GlowEnabled = true;
                        GlowBrush = System.Windows.Application.Current.Resources["AlarmGlowBrush"] as System.Windows.Media.SolidColorBrush;
                        InactiveGlowBrush = System.Windows.Application.Current.Resources["AlarmInactiveGlowBrush"] as System.Windows.Media.SolidColorBrush;
                    }
#endif
                    break;

                case FrameType.WindowsDefault:
                    // Nothing to do here - custom chrome is disabled by default.
                    break;
            }
        }


#if ENFORCE_PACKAGE_SECURITY
        public override void OnApplyTemplate()
        {
            if (_windowManager.Application.Package.Signature == null)
                this.Style = System.Windows.Application.Current.Resources["AlarmParagonWindow"] as Style;
            base.OnApplyTemplate();
        }
#endif

        private void ApplyWindowOptions()
        {
            if (_options == null)
            {
                return;
            }

            var bounds = _options.OuterBounds;
            var frameOptions = _options.Frame;

            if (frameOptions != null)
            {
                if (frameOptions.Type == FrameType.NotSpecified)
                {
                    frameOptions.Type = _windowManager.Application.Package.Manifest.DefaultFrameType;
                }

#if ENFORCE_PACKAGE_SECURITY
                if (_windowManager.Application.Package.Signature == null)
                {
                    if (frameOptions.Type != FrameType.None)
                        frameOptions.Type = FrameType.Paragon;
                }
#endif

                ApplyFrameStyle(frameOptions.Type);

                if (frameOptions.Icon)
                {
                    using (var stream = _windowManager.Application.Package.GetIcon())
                    {
                        if (stream != null)
                        {
                            try
                            {
                                Icon = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(string.Format("Failed to create a bitmap frame from the stream: {0}", ex.Message));
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(_options.InitialState))
            {
                switch (_options.InitialState.ToLower())
                {
                    case "normal":
                        WindowState = WindowState.Normal;
                        break;
                    case "maximized":
                        WindowState = WindowState.Maximized;
                        break;
                    case "minimized":
                        WindowState = WindowState.Minimized;
                        break;
                    case "fullscreen":
                        FullScreenWindow();
                        break;
                }
            }

            if (bounds != null)
            {
                Left = bounds.Left;
                Top = bounds.Top;

                if (bounds.Width > 0)
                {
                    Width = bounds.Width;
                }
                if (bounds.Height > 0)
                {
                    Height = bounds.Height;
                }

                if (bounds.MinWidth > 0)
                {
                    MinWidth = bounds.MinWidth;
                }
                if (bounds.MinHeight > 0)
                {
                    MinHeight = bounds.MinHeight;
                }
                if (bounds.MaxWidth > 0)
                {
                    MaxWidth = bounds.MaxWidth;
                }
                if (bounds.MaxHeight > 0)
                {
                    MaxHeight = bounds.MaxHeight;
                }
            }

            if (!string.IsNullOrEmpty(_title))
            {
                SetTitle(_title);
            }

            ResizeMode = _options.Resizable ? ResizeMode.CanResize : ResizeMode.NoResize;
            MinMaxButtonsVisible = _options.Resizable;

            Topmost = _options.AlwaysOnTop;
            _minimizeOnClose = _options.MinimizeOnClose;

            if (!_options.Focused)
            {
                ShowActivated = false;
            }

            InvalidateArrange();
        }

        private void SetTitle(string title)
        {
#if ENFORCE_PACKAGE_SECURITY
            if (_windowManager.Application.Package.Signature == null)
            {
                Title = title + " (Unsigned)";
            }
            else
#endif
            Title = title;
        }

        private void AttachToBrowser()
        {
            _browser.TitleChanged += TitleChanged;
            _browser.BeforePopup += BeforePopup;
            _browser.ShowPopup += ShowPopup;
            _browser.JSDialog += OnJavaScriptDialog;
            _browser.BeforeUnloadDialog += OnUnloadPageDialog;
            _browser.BrowserClosed += OnBrowserClosed;
            _browser.BeforeResourceLoad += OnBeforeResourceLoad;
            _browser.DownloadUpdated += OnDownloadUpdated;
            _browser.BeforeDownload += OnBeginDownload;
            _browser.ProtocolExecution += OnProtocolExecution;
        }

        internal Size GetRealSize(Vector virtualSize)
        {
            var realSize = new Size(virtualSize.X, virtualSize.Y);

            try
            {
                var source = PresentationSource.FromVisual(this);
                if (source != null && source.CompositionTarget != null)
                {
                    var transformToDevice = source.CompositionTarget.TransformToDevice;
                    realSize = (Size) transformToDevice.Transform((Vector) virtualSize);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to get real size: {0}", ex.Message));
            }

            return realSize;
        }

        internal Size GetVirtualSize(Vector realSize)
        {
            var virtualSize = new Size(realSize.X, realSize.Y);

            try
            {
                var source = PresentationSource.FromVisual(this);
                if (source != null && source.CompositionTarget != null)
                {
                    var transformFromDevice = source.CompositionTarget.TransformFromDevice;
                    virtualSize = (Size)transformFromDevice.Transform((Vector)realSize);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to get virtual size: {0}", ex.Message));
            }

            return virtualSize;
        }

        private void OnProtocolExecution(object sender, ProtocolExecutionEventArgs e)
        {            
            if(_windowManager.Application.Package.Manifest.CustomProtocolWhitelist != null)
            {
                e.Allow = _windowManager.Application.Package.Manifest.CustomProtocolWhitelist.Contains("*") ||
                          _windowManager.Application.Package.Manifest.CustomProtocolWhitelist.Contains(new Uri(e.Url).Scheme);
            }
        }

        private void OnDownloadUpdated(object sender, DownloadProgressEventArgs e)
        {
            if (DownloadProgress != null)
            {
                DownloadProgress(this, e);
                if (!e.IsCanceled && e.IsComplete)
                {
                    if (_browser.Source.Equals(e.Url, StringComparison.InvariantCultureIgnoreCase))
                        this.Close();
                }
            }
        }

        private void OnBeginDownload(object sender, BeginDownloadEventArgs e)
        {
            if (BeginDownload != null)
            {
                BeginDownload(this, e);
            }
        }

        private void OnBeforeResourceLoad(object sender, ResourceLoadEventArgs e)
        {
            e.Cancel = true;

            if (_windowManager != null && _windowManager.Application != null &&
                _windowManager.Application.Package.Manifest.ExternalUrlWhitelist != null)
            {
                var reqUrl = e.Url;
                var whitelist = _windowManager.Application.Package.Manifest.ExternalUrlWhitelist;
                foreach (var url in whitelist)
                {
                    try
                    {
                        if (url.Equals(reqUrl, StringComparison.InvariantCultureIgnoreCase) ||
                            url.Equals("*"))
                        {
                            e.Cancel = false;
                            break;
                        }

                        var pattern = Regex.Escape(url).Replace(@"\*", ".+?").Replace(@"\?", ".");
                        if (pattern.EndsWith(".+?"))
                        {
                            pattern = url.Remove(pattern.Length - 3, 3);
                            pattern += ".*";
                        }

                        var regEx = new Regex(pattern);
                        if (regEx.Match(reqUrl).Success)
                        {
                            e.Cancel = false;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(string.Format("Invalid pattern in ExternalUrlWhitelist itemn {0}: {1}", url, ex.Message));
                    }
                }
            }
        }

        private void BeforePopup(object sender, BeforePopupEventArgs eventArgs)
        {
            _windowManager.BeforeApplicationWindowPopup(eventArgs);
        }

        private void DetachFromBrowser()
        {
            _browser.LoadEnd -= OnPageLoaded;
            _browser.TitleChanged -= TitleChanged;
            _browser.BeforePopup -= BeforePopup;
            _browser.ShowPopup -= ShowPopup;
            _browser.JSDialog -= OnJavaScriptDialog;
            _browser.BeforeUnloadDialog -= OnUnloadPageDialog;
            _browser.BeforeResourceLoad -= OnBeforeResourceLoad;
            _browser.DownloadUpdated -= OnDownloadUpdated;
            _browser.BeforeDownload -= OnBeginDownload;
            _browser.ProtocolExecution -= OnProtocolExecution;
        }

        private void DoCleanup()
        {
            if (_browser != null)
            {
                DetachFromBrowser();
                _browser.BrowserClosed -= OnBrowserClosed;
                _browser.Dispose();
                _browser = null;
                if (_windowManager != null)
                {
                    _windowManager.RemoveApplicationWindow(this);
                    _windowManager = null;
                }
                Content = null;
            }

            if (_autoSaveWindowPositionBehavior != null)
            {
                _autoSaveWindowPositionBehavior.Detach();
            }
        }

        private void OnBrowserClosed(object sender, EventArgs e)
        {
            if (_browser != null)
            {
                _isClosed = true;
                DoCleanup();
                Close();
            }
        }

        private void OnBrowserPreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyPress(e.Key);
        }

        private void OnJavaScriptDialog(object sender, JsDialogEventArgs ea)
        {
            try
            {
                // Bring up the dialog asynchronously
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var result = MessageBoxResult.Cancel;
                    var msg = string.Empty;
                    switch (ea.DialogType)
                    {
                        case CefJSDialogType.Alert:
                            JavaScriptDialog.Alert("Alert", ea.MessageText, this);
                            result = MessageBoxResult.OK;
                            ea.Handled = true;
                            break;
                        case CefJSDialogType.Confirm:
                            result = JavaScriptDialog.Confirm("Confirm", ea.MessageText, this);
                            break;
                        case CefJSDialogType.Prompt:
                            msg = ea.DefaultPromptText;
                            result = JavaScriptDialog.Prompt("Prompt", ea.MessageText, ref msg, this);
                            break;
                    }
                    if (_browser != null)
                    {
                        ea.Callback.Continue(result == MessageBoxResult.OK, msg);
                    }
                }));
            }
            finally
            {
                ea.Handled = true;
            }
        }

        private void OnPageLoaded(object sender, LoadEndEventArgs e)
        {
            Logger.Info("OnPageLoaded : Firing PageLoaded");
            SetBrowserAsContent();
            if (PageLoaded != null)
                PageLoaded(e.Url);
        }

        void SetBrowserAsContent()
        {
            if (Content == null && _browser.BrowserWindowHandle != IntPtr.Zero)
            {
                Content = _browser;
                LoadComplete.Raise(this, EventArgs.Empty);
                InvalidateArrange();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += OnSizeChanged;
            LocationChanged += OnLocationChanged;
            SystemEvents.SessionEnding += OnSessionEnding;
            SetBrowserAsContent();
        }

        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            Close();
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            if (_isClosing || _isClosed)
                return;
            var bounds = GetBounds();
            WindowBoundsChanged.Raise(() => new object[] { bounds });
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isClosing || _isClosed)
                return;
            var bounds = GetBounds();
            WindowBoundsChanged.Raise(() => new object[] { bounds });
        }

        private BoundsSpecification GetBounds()
        {
            if( Dispatcher.CheckAccess() )
            {
                var rect = RECT.FromHandle(Handle);
                var screen = Screen.FromHandle(Handle);
                var workingAreaRect = screen.WorkingArea;

                return new BoundsSpecification
                {
                    Left = (rect.Right > workingAreaRect.Right) ? (workingAreaRect.Right - rect.Width) : (rect.Left),
                    Top = (rect.Bottom > workingAreaRect.Bottom) ? (workingAreaRect.Bottom - rect.Height) : (rect.Top),
                    Height = rect.Height,
                    Width = rect.Width,
                    MaxHeight = MaxHeight,
                    MaxWidth = MaxWidth,
                    MinHeight = MinHeight,
                    MinWidth = MinWidth
                };
            }
            return (BoundsSpecification)Dispatcher.Invoke(new Func<BoundsSpecification>(GetBounds));
        }

        private void OnUnloadPageDialog(object sender, UnloadDialogEventArgs ea)
        {
            try
            {
                // Bring up the dialog asynchronously
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var msg = string.Empty;
                    var r = JavaScriptDialog.Confirm("JavaScript", ea.MessageText, this);
                    if (_browser != null)
                    {
                        _isClosing = (r == MessageBoxResult.OK);
                        ea.Callback.Continue(_isClosing, msg);
                    }
                }));
            }
            finally
            {
                ea.Handled = true;
            }
        }

        private void Reload(bool ignoreCache)
        {
            _browser.Reload(ignoreCache);
        }

        private void ShowPopup(object sender, ShowPopupEventArgs eventArgs)
        {
            _windowManager.ShowApplicationWindowPopup(this, eventArgs);
        }

        private void TitleChanged(object sender, TitleChangedEventArgs e)
        {
            if (_windowManager !=null)
            {
                if(_windowManager.Application.Package.Manifest.UseAppNameAsWindowTitle)
                    return;
            }

            SetTitle(e.Title ?? string.Empty);
            PropertyChanged.Raise(() => new object[] { "title", e.Title });
        }

        private void ToolsClosed(object sender, EventArgs ea)
        {
            if (_tools != null)
            {
                _tools.Closed -= ToolsClosed;
            }

            _tools = null;
        }

        private void HandleKeyPress(Key key)
        {
            switch (key)
            {
                case Key.J:
                    if ((key & Key.LeftCtrl) == Key.LeftCtrl &&
                        (key & Key.LeftCtrl) == Key.LeftShift)
                    {
                        ShowDeveloperTools(new Point(int.MinValue, int.MinValue));
                    }
                    break;

                case Key.F12:
                    ShowDeveloperTools(new Point(int.MinValue, int.MinValue));
                    break;

                case Key.F5:
                    Reload((key & Key.LeftCtrl) == Key.LeftCtrl);
                    break;

                case Key.Escape:
                    if (IsFullScreenEnabled)
                    {
                        Restore();
                    }
                    break;
            }
        }

        [DebuggerStepThrough]
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_minimizeOnClose && msg == (int)WM.CLOSE)
            {
                Minimize();
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tools != null)
                {
                    _tools.Dispose();
                    _tools = null;
                }
            }
        }

        private void DispatchIfRequired(Action a, bool isAsync = false)
        {
            if (_browser != null)
            {
                if (!Dispatcher.CheckAccess())
                {
                    if (isAsync)
                    {
                        Dispatcher.BeginInvoke(a);
                    }
                    else
                    {
                        Dispatcher.Invoke(a);
                    }
                }
                else
                {
                    a();
                }
            }
        }

        private T DispatchIfRequired<T>(Func<T> a)
        {
            if (_browser != null)
            {
                if (!Dispatcher.CheckAccess())
                {
                    return (T)Dispatcher.Invoke(a);
                }
                return a();
            }
            return default(T);
        }

        public class DeveloperToolsWindow : ApplicationWindow
        {
            private BrowserHwndHost _control;
            public void Initialize(IDisposable control, string title, FrameType frameType)
            {
                Title = "Paragon Developer Tools - " + title;
                _control = control as BrowserHwndHost;
                if (_control != null)
                {
                    _control.HandleCreated += OnControlHandleCreated;
                }
                Loaded += OnWindowLoaded;
                ApplyFrameStyle(frameType);
            }

            void OnWindowLoaded(object sender, RoutedEventArgs e)
            {
                if (Handle != IntPtr.Zero)
                {
                    OnControlHandleCreated(this, EventArgs.Empty);
                }
            }

            void OnControlHandleCreated(object sender, EventArgs e)
            {
                _control.Width = double.NaN;
                _control.Height = double.NaN;
                Content = _control;
                InvalidateArrange();
            }

            protected override void OnClosed(EventArgs e)
            {
                var control = Content as IDisposable;
                if (control != null)
                {
                    control.Dispose();
                }
                Loaded -= OnWindowLoaded;
                base.OnClosed(e);
            }

            protected override void OnClosing(CancelEventArgs e)
            {
                // Required to prevent the base class logic that handles closing.
            }
        }
    }
}