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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Paragon.Runtime.Win32;
using Paragon.Runtime.WPF;
using Paragon.Runtime;
using Paragon.Plugins;

namespace Paragon
{
    /// <summary>
    /// Interaction logic for ParagonSplashScreen.xaml
    /// </summary>
    public partial class ParagonSplashScreen : INotifyPropertyChanged, IParagonSplashScreen
    {
        private Storyboard _showboard;
        private Storyboard _hideboard;
        private FrameworkElement _statusText;
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();

        static ParagonSplashScreen()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ParagonSplashScreen), new FrameworkPropertyMetadata(typeof(ParagonSplashScreen)));
        }

        public ParagonSplashScreen(string shellName, string version, Stream shellIconStream)
        {
            InitializeComponent();
            ShellName = shellName;
            Version = version;
            Text = "Initializing...";
            DataContext = this;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.FullPrimaryScreenHeight;
            Left = (screenWidth / 2) - (Width / 2);
            Top = (screenHeight / 2) - (Height / 2);

            if (shellIconStream != null)
            {
                try
                {
                    var icon = new Icon(shellIconStream, 128, 128);

                    using (var bmp = icon.ToBitmap())
                    {
                        var hbmp = bmp.GetHbitmap();

                        try
                        {
                            ShellIcon = Imaging.CreateBitmapSourceFromHBitmap(hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error creating splash screen bitmap: ", ex);
                        }
                        finally
                        {
                            if (IntPtr.Zero != hbmp)
                                Win32Api.DeleteObject(hbmp);
                        }
                    }
                }
                catch
                {
                    Logger.Warn("Could not create the splash screen icon from the specified icon in the manifest. Will use the paragon icon ");
                }
            }

            if (ShellIcon == null)
                ShellIcon = new IconImage(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window.ico")).ImageSource;
        }

        public override void OnApplyTemplate()
        {
            if (Application.Current.Resources.Contains("CustomSplashScreenStyle"))
            {
                Style = Application.Current.Resources["CustomSplashScreenStyle"] as Style;
            }

            base.OnApplyTemplate();

            if (Template != null)
            {
                _showboard = Style.Resources["ShowStoryBoard"] as Storyboard;
                _hideboard = Style.Resources["HideStoryBoard"] as Storyboard;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string ShellName { get; set; }
        public ImageSource ShellIcon { get; private set; }
        public string Version { get; set; }
        public string Text { get; private set; }
        public string PreviousText { get; private set; }

        public void ShowText(string text)
        {
            if (_statusText == null)
                _statusText = GetTemplateChild("StatusText") as FrameworkElement;

            PreviousText = Text;
            Text = text;

            if (_showboard != null && _statusText != null)
                _showboard.Begin(_statusText);

            OnPropertyChanged("Text");
            OnPropertyChanged("PreviousText");

            if (_hideboard != null && _statusText != null)
                _hideboard.Begin(_statusText);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}