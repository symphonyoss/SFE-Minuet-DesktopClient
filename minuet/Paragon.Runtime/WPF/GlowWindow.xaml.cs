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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WPF
{
    partial class GlowWindow
    {
        private const double EdgeSize = 20.0;
        private const double GlowSize = 9.0;
        private readonly Func<Point, Cursor> _getCursor;
        private readonly Func<RECT, double> _getHeight;
        private readonly Func<Point, HT> _getHitTestValue;
        private readonly Func<RECT, double> _getLeft;
        private readonly Func<RECT, double> _getTop;
        private readonly Func<RECT, double> _getWidth;
        private IntPtr _handle;
        private HwndSource _hwndSource;
        private IntPtr _ownerHandle;

        public GlowWindow(Window owner, GlowDirection direction)
        {
            InitializeComponent();

            Owner = owner;
            glow.Visibility = Visibility.Collapsed;
            glow.IsGlow = false;

            var b = new Binding("GlowBrush") {Source = owner};
            glow.SetBinding(Glow.GlowBrushProperty, b);

            var b2 = new Binding("InactiveGlowBrush") { Source = owner };
            glow.SetBinding(Glow.InactiveGlowBrushProperty, b);

            switch (direction)
            {
                case GlowDirection.Left:
                    glow.Orientation = Orientation.Vertical;
                    glow.HorizontalAlignment = HorizontalAlignment.Right;
                    _getLeft = rect => Math.Round(rect.Left - GlowSize);
                    _getTop = rect => Math.Round(rect.Top - GlowSize);
                    _getWidth = rect => GlowSize;
                    _getHeight = rect => rect.Height + GlowSize*2.0;

                    _getHitTestValue = p => new Rect(0, 0, ActualWidth, EdgeSize).Contains(p)
                        ? HT.TOPLEFT
                        : new Rect(0, ActualHeight - EdgeSize, ActualWidth, EdgeSize).Contains(p)
                            ? HT.BOTTOMLEFT
                            : HT.LEFT;

                    _getCursor = p => new Rect(0, 0, ActualWidth, EdgeSize).Contains(p)
                        ? Cursors.SizeNWSE
                        : new Rect(0, ActualHeight - EdgeSize, ActualWidth, EdgeSize).Contains(p)
                            ? Cursors.SizeNESW
                            : Cursors.SizeWE;
                    break;

                case GlowDirection.Right:
                    glow.Orientation = Orientation.Vertical;
                    glow.HorizontalAlignment = HorizontalAlignment.Left;
                    _getLeft = rect => rect.Right;
                    _getTop = rect => Math.Round((rect.Top - GlowSize));
                    _getWidth = rect => GlowSize;
                    _getHeight = rect => (rect.Height + GlowSize*2.0);

                    _getHitTestValue = p => new Rect(0, 0, ActualWidth, EdgeSize).Contains(p)
                        ? HT.TOPRIGHT
                        : new Rect(0, ActualHeight - EdgeSize, ActualWidth, EdgeSize).Contains(p)
                            ? HT.BOTTOMRIGHT
                            : HT.RIGHT;

                    _getCursor = p => new Rect(0, 0, ActualWidth, EdgeSize).Contains(p)
                        ? Cursors.SizeNESW
                        : new Rect(0, ActualHeight - EdgeSize, ActualWidth, EdgeSize).Contains(p)
                            ? Cursors.SizeNWSE
                            : Cursors.SizeWE;
                    break;

                case GlowDirection.Top:
                    glow.Orientation = Orientation.Horizontal;
                    glow.VerticalAlignment = VerticalAlignment.Bottom;
                    _getLeft = rect => rect.Left;
                    _getTop = rect => Math.Round(rect.Top - GlowSize);
                    _getWidth = rect => rect.Width;
                    _getHeight = rect => GlowSize;

                    _getHitTestValue = p => new Rect(0, 0, EdgeSize - GlowSize, ActualHeight).Contains(p)
                        ? HT.TOPLEFT
                        : new Rect(Width - EdgeSize + GlowSize, 0, EdgeSize - GlowSize, ActualHeight).Contains(p)
                            ? HT.TOPRIGHT
                            : HT.TOP;

                    _getCursor = p => new Rect(0, 0, EdgeSize - GlowSize, ActualHeight).Contains(p)
                        ? Cursors.SizeNWSE
                        : new Rect(Width - EdgeSize + GlowSize, 0, EdgeSize - GlowSize, ActualHeight).
                            Contains(p)
                            ? Cursors.SizeNESW
                            : Cursors.SizeNS;
                    break;

                case GlowDirection.Bottom:
                    glow.Orientation = Orientation.Horizontal;
                    glow.VerticalAlignment = VerticalAlignment.Top;
                    _getLeft = rect => rect.Left;
                    _getTop = rect => rect.Bottom;
                    _getWidth = rect => rect.Width;
                    _getHeight = rect => GlowSize;

                    _getHitTestValue = p => new Rect(0, 0, EdgeSize - GlowSize, ActualHeight).Contains(p)
                        ? HT.BOTTOMLEFT
                        : new Rect(Width - EdgeSize + GlowSize, 0, EdgeSize - GlowSize,
                            ActualHeight).Contains(p)
                            ? HT.BOTTOMRIGHT
                            : HT.BOTTOM;

                    _getCursor = p => new Rect(0, 0, EdgeSize - GlowSize, ActualHeight).Contains(p)
                        ? Cursors.SizeNESW
                        : new Rect(Width - EdgeSize + GlowSize, 0, EdgeSize - GlowSize, ActualHeight).
                            Contains(p)
                            ? Cursors.SizeNWSE
                            : Cursors.SizeNS;
                    break;
            }

            if (Owner.IsLoaded)
            {
                glow.Visibility = Visibility.Visible;
                glow.IsGlow = Owner.IsActive;
            }
            else
            {
                Owner.ContentRendered += OwnerOnContentRendered;
            }

            Owner.Activated += OwnerOnStateChanged;
            Owner.Deactivated += OwnerOnStateChanged;
            Owner.LocationChanged += OwnerOnStateChanged;
            Owner.SizeChanged += OwnerOnStateChanged;
            Owner.StateChanged += OwnerOnStateChanged;
            Owner.Closed += OwnerOnClosed;
        }

        public Storyboard OpacityStoryboard { get; set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Owner.Activated -= OwnerOnStateChanged;
            Owner.Deactivated -= OwnerOnStateChanged;
            Owner.LocationChanged -= OwnerOnStateChanged;
            Owner.SizeChanged -= OwnerOnStateChanged;
            Owner.StateChanged -= OwnerOnStateChanged;
            Owner.Closed -= OwnerOnClosed;
            _hwndSource.RemoveHook(WndProc);
            _hwndSource.Dispose();
        }

        private void OwnerOnClosed(object sender, EventArgs e)
        {
            Close();
        }

        private void OwnerOnContentRendered(object sender, EventArgs e)
        {
            glow.Visibility = Visibility.Visible;
            if (Owner.IsActive)
            {
                glow.IsGlow = true;
            }

            Owner.ContentRendered -= OwnerOnContentRendered;
        }

        private void OwnerOnStateChanged(object sender, EventArgs e)
        {
            if (Owner.IsActive && !glow.IsGlow)
            {
                glow.IsGlow = true;
            }
            else if (!Owner.IsActive && glow.IsGlow)
            {
                glow.IsGlow = false;
            }
            else
            {
                Update();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OpacityStoryboard = TryFindResource("OpacityStoryboard") as Storyboard;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hwndSource = (HwndSource) PresentationSource.FromVisual(this);
            if (_hwndSource == null)
            {
                throw new Exception("Error getting glow window handle");
            }

            _hwndSource.AddHook(WndProc);
            _handle = _hwndSource.Handle;
        }

        public void Update()
        {
            if (Owner.IsVisible && Owner.WindowState == WindowState.Normal)
            {
                if (!IsVisible)
                {
                    Visibility = Visibility.Visible;
                }

                UpdateCore();
            }
            else
            {
                Visibility = Visibility.Hidden;
            }
        }

        private void UpdateCore()
        {
            if (_ownerHandle == IntPtr.Zero)
            {
                _ownerHandle = new WindowInteropHelper(Owner).Handle;

                if (_ownerHandle == IntPtr.Zero)
                {
                    return;
                }

                Win32Api.SetWindowOwner(_handle, _ownerHandle);
            }

            var rect = RECT.FromHandle(_ownerHandle);

            Win32Api.SetWindowPosition(
                _handle,
                _ownerHandle,
                (int) _getLeft(rect),
                (int) _getTop(rect),
                (int) _getWidth(rect),
                (int) _getHeight(rect),
                SWP.NOACTIVATE);
        }

        [DebuggerStepThrough]
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int) WM.MOUSEACTIVATE) // 33
            {
                handled = true;
                return new IntPtr(3);
            }

            switch (msg)
            {
                case (int) WM.LBUTTONDOWN:
                {
                    var pt = new Point((int) lParam & 0xFFFF, ((int) lParam >> 16) & 0xFFFF);
                    var ht = (int) _getHitTestValue(pt);
                    Win32Api.PostMessage(_ownerHandle, (int) WM.NCLBUTTONDOWN, new IntPtr(ht), IntPtr.Zero);
                }
                    break;

                case (int) WM.NCHITTEST:
                {
                    var ptScreen = new Point((int) lParam & 0xFFFF, ((int) lParam >> 16) & 0xFFFF);
                    var ptClient = PointFromScreen(ptScreen);
                    Cursor = _getCursor(ptClient);
                }
                    break;
            }

            return IntPtr.Zero;
        }
    }
}