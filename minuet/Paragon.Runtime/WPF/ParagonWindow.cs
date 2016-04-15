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
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Windows.Shell;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WPF
{
    public class ParagonWindow : Window
    {
        public static readonly DependencyProperty ShowIconInTitleBarProperty = DependencyProperty.Register("ShowIconInTitleBar", typeof (bool), typeof (ParagonWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty GlowBrushProperty = DependencyProperty.Register("GlowBrush", typeof (SolidColorBrush), typeof (ParagonWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty InactiveGlowBrushProperty = DependencyProperty.Register("InactiveGlowBrush", typeof(SolidColorBrush), typeof(ParagonWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty GlowEnabledProperty = DependencyProperty.Register("GlowEnabled", typeof(bool), typeof(ParagonWindow), new PropertyMetadata(true, OnGlowEnabledChanged));
        public static readonly DependencyProperty TitlebarHeightProperty = DependencyProperty.Register("TitlebarHeight", typeof (double), typeof (ParagonWindow), new PropertyMetadata(25.0d));
        public static readonly DependencyProperty TitlebarFontSizeProperty = DependencyProperty.Register("TitlebarFontSize", typeof (double), typeof (ParagonWindow), new PropertyMetadata(14.0d));
        public static readonly DependencyProperty MinMaxButtonsVisibleProperty = DependencyProperty.Register("MinMaxButtonsVisible", typeof (bool), typeof (ParagonWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty WindowTitleVisibleProperty = DependencyProperty.Register("WindowTitleVisible", typeof (bool), typeof (ParagonWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty CustomChromeEnabledProperty = DependencyProperty.Register("CustomChromeEnabled", typeof (bool), typeof (ParagonWindow), new PropertyMetadata(false, OnCustomChromeEnabledChanged));

        private readonly Lazy<IntPtr> _hwnd;
        private GlowWindowBehavior _glowBehavior;
        private bool _isFullScreen;
        private WINDOWPLACEMENT _previousPlacement;
        private double _previousTitleBarHeight;

        static ParagonWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof (ParagonWindow), new FrameworkPropertyMetadata(typeof (ParagonWindow)));
        }

        public ParagonWindow()
        {
            _hwnd = new Lazy<IntPtr>(() => new WindowInteropHelper(this).EnsureHandle());
        }

        public SolidColorBrush GlowBrush
        {
            get { return (SolidColorBrush) GetValue(GlowBrushProperty); }
            set { SetValue(GlowBrushProperty, value); }
        }

        public SolidColorBrush InactiveGlowBrush
        {
            get { return (SolidColorBrush)GetValue(InactiveGlowBrushProperty); }
            set { SetValue(InactiveGlowBrushProperty, value); }
        }

        public bool GlowEnabled
        {
            get { return (bool) GetValue(GlowEnabledProperty); }
            set { SetValue(GlowEnabledProperty, value); }
        }

        public bool CustomChromeEnabled
        {
            get { return (bool) GetValue(CustomChromeEnabledProperty); }
            set { SetValue(CustomChromeEnabledProperty, value); }
        }

        public bool MinMaxButtonsVisible
        {
            get { return (bool) GetValue(MinMaxButtonsVisibleProperty); }
            set { SetValue(MinMaxButtonsVisibleProperty, value); }
        }

        public bool ShowIconInTitleBar
        {
            get { return (bool) GetValue(ShowIconInTitleBarProperty); }
            set { SetValue(ShowIconInTitleBarProperty, value); }
        }

        public double TitlebarHeight
        {
            get { return (double) GetValue(TitlebarHeightProperty); }
            set { SetValue(TitlebarHeightProperty, value); }
        }

        public int TitlebarFontSize
        {
            get { return (int) GetValue(TitlebarFontSizeProperty); }
            set { SetValue(TitlebarFontSizeProperty, value); }
        }

        public bool WindowTitleVisible
        {
            get { return (bool) GetValue(WindowTitleVisibleProperty); }
            set { SetValue(WindowTitleVisibleProperty, value); }
        }

        public WindowCommands WindowCommands { get; set; }

        public bool IsFullScreenEnabled
        {
            get { return _isFullScreen; }

            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                if (value == _isFullScreen)
                {
                    return;
                }

                _isFullScreen = value;
                if (_isFullScreen)
                {
                    _previousTitleBarHeight = TitlebarHeight;
                    TitlebarHeight = 0;
                    _previousPlacement = Win32Api.FullScreen(_hwnd.Value);
                }
                else
                {
                    TitlebarHeight = _previousTitleBarHeight;
                    Win32Api.Restore(_hwnd.Value, ref _previousPlacement);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            if (Application.Current.Resources.Contains("CustomFrameWindowStyle"))
            {
                Style = Application.Current.Resources["CustomFrameWindowStyle"] as Style;
            }

            var minimizeButton = GetTemplateChild("minimizeButton") as Button;
            if (minimizeButton != null)
            {
                minimizeButton.Click += MinimizeClick;
            }

            var restoreButton = GetTemplateChild("restoreButton") as Button;
            if (restoreButton != null)
            {
                restoreButton.Click += RestoreClick;
            }

            var closeButton = GetTemplateChild("closeButton") as Button;
            if (closeButton != null)
            {
                closeButton.Click += CloseClick;
            }

            if (WindowCommands == null)
            {
                WindowCommands = new WindowCommands();
            }

            if (GlowEnabled)
            {
                _glowBehavior = new GlowWindowBehavior();
                _glowBehavior.Attach(this);
            }

            // VSADDA - Window Snap Behavior is not taking into account maximize and minimize when processing WM_MOVE events
            //          Disabling it until this is fixed correctly. This is a fix for PARAGONDP-830
            // var snapBehavior = new WindowSnapBehavior();
            // snapBehavior.Attach(this);

            base.OnApplyTemplate();
        }

        public void StartDragMove()
        {
            Win32Api.DragMove(_hwnd.Value);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Flash(bool clear = false, bool autoclear = false, int maxFlashes = 5, int timeOut = 0)
        {
            Win32Api.FlashWindow(_hwnd.Value, clear, autoclear, maxFlashes, timeOut);
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RestoreClick(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private static void OnCustomChromeEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = (ParagonWindow)dependencyObject;
            window.SetCustomChromeEnabled((bool)e.NewValue);
        }

        private void SetCustomChromeEnabled(bool enabled)
        {
            // The WindowChrome type has issues when run on Win2K3 for a WPF window that 
            // contains a hosted winforms control. Setting WindowChrome dependency properties 
            // in XAML in that scenario triggers access violation execptions, so we are setting
            // the properties programmatically here. The same access violation exception will
            // occur if this method (EnableCustomWindowChrome) is JIT'd at runtime. It will not 
            // be JIT'd as long as conditional that determines whether the current OS version is 
            // Win7 or greater (WindowsVersion.IsWin7OrNewer) is evaluated outside of this method. 
            if (enabled)
            {
                var chrome = new WindowChrome
                {
                    CaptionHeight = TitlebarHeight,
                    CornerRadius = new CornerRadius(0),
                    ResizeBorderThickness = new Thickness(0)
                };

                WindowChrome.SetWindowChrome(this, chrome);
            }
        }

        private static void OnGlowEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = (ParagonWindow) dependencyObject;
            window.SetGlowEnabled((bool) e.NewValue);
        }

        private void SetGlowEnabled(bool enabled)
        {
            if (enabled)
            {
                if (_glowBehavior == null)
                {
                    _glowBehavior = new GlowWindowBehavior();
                    _glowBehavior.Attach(this);
                }
            }
            else
            {
                if (_glowBehavior != null)
                {
                    _glowBehavior.Detach();
                    _glowBehavior = null;
                }
            }
        }
    }
}