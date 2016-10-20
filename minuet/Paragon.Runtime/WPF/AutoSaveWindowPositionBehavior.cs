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
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Interactivity;
using System.Windows.Interop;
using Microsoft.Win32;
using Paragon.Plugins;
using Paragon.Runtime.Kernel.Windowing;
using Paragon.Runtime.Win32;
using System.Windows;

namespace Paragon.Runtime.WPF
{
    internal class AutoSaveWindowPositionBehavior : Behavior<ApplicationWindow>
    {
        private const string StorageDirectoryName = "paragon.window.position";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private IntPtr? _hwnd;
        private IsolatedStorageFile _store;
        private string _storageFilePath;
        private bool _blockChanges;
        private RECT? _initialWindowPlacement;

        public AutoSaveWindowPositionBehavior(RECT? initialWindowPlacement)
        {
            _initialWindowPlacement = initialWindowPlacement;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            try
            {
                var fileName = string.Format("{0}.{1}", AssociatedObject.GetAppId(), AssociatedObject.GetId());
                _storageFilePath = Path.Combine(StorageDirectoryName, fileName);
                _store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                _store.CreateDirectory(StorageDirectoryName);
            }
            catch (Exception e)
            {
                Logger.Error("Error creating isolated storage file for saving window position", e);
                return;
            }

            AssociatedObject.LocationChanged += OnSizeOrLocationChanged;
            AssociatedObject.SizeChanged += OnSizeOrLocationChanged;
            AssociatedObject.Activated += OnActivated;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

            if (AssociatedObject.IsInitialized)
            {
                Init();
            }
            else
            {
                AssociatedObject.SourceInitialized += (s, e) => Init();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            try
            {
                AssociatedObject.LocationChanged -= OnSizeOrLocationChanged;
                AssociatedObject.SizeChanged -= OnSizeOrLocationChanged;
                AssociatedObject.Activated -= OnActivated;
                SystemEvents.SessionSwitch -= OnSessionSwitch;
                SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            }
            catch (Exception e)
            {
                Logger.Error("Error cleaning up on detach", e);                
            }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Logger.Debug("Blocking window size and location changes as session is being locked.");
                _blockChanges = true;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Logger.Debug("Unblocking window size and location changes as session is being unlocked.");
                _blockChanges = false;
            }
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            Logger.Debug("Display settings changed. Blocking window size and location changes, applying saved window placement settings.");
            _blockChanges = true;
            ApplyWindowPlacement();
        }

        private void OnSizeOrLocationChanged(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                Logger.Debug("Size or location changed. Persisting window placement settings.");
                PersistWindowPlacement();
            }
        }

        private void OnActivated(object sender, EventArgs e)
        {
            Logger.Debug("Unblocking window size and location changes as window has been activated.");
            _blockChanges = false;
        }

        private void PersistWindowPlacement()
        {
            if (!_hwnd.HasValue)
            {
                return;
            }

            try
            {
                var placement = default(WINDOWPLACEMENT);
                if (NativeMethods.GetWindowPlacement(_hwnd.Value, ref placement))
                {
                    Logger.Debug(string.Format("Placement details being saved (real): Top {0} Left {1} Width {2} Height {3} State {4}",
                        placement.rcNormalPosition.Top, placement.rcNormalPosition.Left, placement.rcNormalPosition.Width, placement.rcNormalPosition.Height, placement.showCmd));

                    UpdateWindowPlacement(ref placement);
                    
                    using (var writer = new StreamWriter(new IsolatedStorageFileStream(_storageFilePath, FileMode.Create, FileAccess.Write, _store)))
                    {
                        writer.Write(StructConverter.ToString(placement));
                        Logger.Debug(string.Format("Placement details being saved (virtual): Top {0} Left {1} Width {2} Height {3} State {4}",
                            placement.rcNormalPosition.Top, placement.rcNormalPosition.Left, placement.rcNormalPosition.Width, placement.rcNormalPosition.Height, placement.showCmd));
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving window position", ex);
            }
        }

        private void ApplyWindowPlacement(RECT placement)
        {
            try
            {
                Logger.Info(string.Format("Placement being applied (virtual): Top {0} Left {1} Width {2} Height {3}",
                    placement.Top, placement.Left, placement.Width, placement.Height));

                var ddPoint = AssociatedObject.GetDeviceDependentPoint(new Point(placement.Left, placement.Top));
                var ddSize = AssociatedObject.GetDeviceDependentSize(new Vector(placement.Width, placement.Height));

                Logger.Info(string.Format("Placement details being applied (real): X {0} Y {1} Width {2} Height {3}",
                    ddPoint.X, ddPoint.Y, ddSize.Width, ddSize.Height));

                int x = (int)ddPoint.X;
                int y = (int)ddPoint.Y;
                int width = (int)ddSize.Width;
                int height = (int)ddSize.Height;

                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x, y, width, height);
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromRectangle(rect);

                // don't want window to overlap on other windows or be displayed off screen, 
                // so by default put into primary window and at (0,0)
                if (!screen.WorkingArea.Contains(rect))
                {
                    x = 0;
                    y = 0;
                }
                
                NativeMethods.SetWindowPos(_hwnd.Value, IntPtr.Zero, x, y, width, height, SWP.SHOWWINDOW);
            }
            catch (Exception e)
            {
                Logger.Error("Error setting window position", e);
            }
        }

        private void ApplyWindowPlacement()
        {
            if (!_hwnd.HasValue)
            {
                return;
            }

            try
            {
                WINDOWPLACEMENT placement;
                if (TryGetPosition(out placement))
                {
                    Logger.Info(string.Format("Placement details being applied (virtual): Top {0} Left {1} Width {2} Height {3} State {4}",
                        placement.rcNormalPosition.Top, placement.rcNormalPosition.Left, placement.rcNormalPosition.Width, placement.rcNormalPosition.Height, placement.showCmd));

                    var ddPoint = AssociatedObject.GetDeviceDependentPoint(new Point(placement.rcNormalPosition.Left, placement.rcNormalPosition.Top));
                    var ddSize = AssociatedObject.GetDeviceDependentSize(new Vector(placement.rcNormalPosition.Width, placement.rcNormalPosition.Height));
                    placement.rcNormalPosition.Left = (int)ddPoint.X;
                    placement.rcNormalPosition.Top = (int)ddPoint.Y;
                    placement.rcNormalPosition.Right = placement.rcNormalPosition.Left + (int)ddSize.Width;
                    placement.rcNormalPosition.Bottom = placement.rcNormalPosition.Top + (int)ddSize.Height;

                    Logger.Info(string.Format("Placement details being applied (real): Top {0} Left {1} Width {2} Height {3} State {4}",
                        placement.rcNormalPosition.Top, placement.rcNormalPosition.Left, placement.rcNormalPosition.Width, placement.rcNormalPosition.Height, placement.showCmd));

                    NativeMethods.SetWindowPlacement(_hwnd.Value, ref placement);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error applying saved window position", e);
            }
        }

        private void Init()
        {
            try
            {
                _hwnd = new WindowInteropHelper(AssociatedObject).Handle;
                if (_initialWindowPlacement == null)
                    ApplyWindowPlacement();
                else
                {
                    RECT placement = _initialWindowPlacement.Value;
                    ApplyWindowPlacement(placement);

                }
            }
            catch (Exception e)
            {
                Logger.Error("Error initializing window auto-save behavior", e);
            }
        }

        private bool TryGetPosition(out WINDOWPLACEMENT placement)
        {
            try
            {
                placement = default(WINDOWPLACEMENT);
                using (var reader = new StreamReader(new IsolatedStorageFileStream(_storageFilePath, FileMode.OpenOrCreate, _store)))
                {
                    var str = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(str))
                    {
                        placement = StructConverter.FromString<WINDOWPLACEMENT>(str);
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error reading saved window position", e);
                placement = default(WINDOWPLACEMENT);
                return false;
            }
        }

        /// <summary>
        /// Updates window placement with virtual settings
        /// GetWindowPlacement does not work as expected in aero snapping cases
        /// </summary>
        /// <param name="placement"></param>
        private void UpdateWindowPlacement(ref WINDOWPLACEMENT placement)
        {
            if (!_hwnd.HasValue)
            {
                return;
            }

            if (placement.showCmd == WindowVisibility.Normal || placement.showCmd == WindowVisibility.Maximized)
            {
                Logger.Debug(string.Format("Window state is {0}. Using get window rectangle to update window placement.", placement.showCmd));

                RECT rectangle;
                if (NativeMethods.GetWindowRect(_hwnd.Value, out rectangle))
                {
                    placement.rcNormalPosition.Top = rectangle.Top;
                    placement.rcNormalPosition.Left = rectangle.Left;
                    placement.rcNormalPosition.Bottom = rectangle.Bottom;
                    placement.rcNormalPosition.Right = rectangle.Right;
                }

                //convert device dependent settings to device independent settings
                var didPoint = AssociatedObject.GetDeviceIndependentPoint(new Point(placement.rcNormalPosition.Left, placement.rcNormalPosition.Top));
                var didSize = AssociatedObject.GetDeviceIndependentSize(new Vector(placement.rcNormalPosition.Width, placement.rcNormalPosition.Height));
                placement.rcNormalPosition.Left = (int)didPoint.X;
                placement.rcNormalPosition.Top = (int)didPoint.Y;
                placement.rcNormalPosition.Right = placement.rcNormalPosition.Left + (int)didSize.Width;
                placement.rcNormalPosition.Bottom = placement.rcNormalPosition.Top + (int)didSize.Height;
            }
            else
            {
                Logger.Debug(string.Format("Window state is {0}. Using persisted window placement to update window placement.", placement.showCmd));

                //persisted settings are virtual
                WINDOWPLACEMENT persistedWindowPlacement;
                if (TryGetPosition(out persistedWindowPlacement))
                {
                    placement.rcNormalPosition.Top = persistedWindowPlacement.rcNormalPosition.Top;
                    placement.rcNormalPosition.Left = persistedWindowPlacement.rcNormalPosition.Left;
                    placement.rcNormalPosition.Bottom = persistedWindowPlacement.rcNormalPosition.Bottom;
                    placement.rcNormalPosition.Right = persistedWindowPlacement.rcNormalPosition.Right;
                }
            }
        }
    }
}