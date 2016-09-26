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
using System.Windows;
using System.Windows.Interop;
using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications.Views
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private readonly IntPtr handle;

        public NotificationWindow()
        {
            InitializeComponent();
            handle = new WindowInteropHelper(this).EnsureHandle();
        }

		//DES-11128
        public void ShowOnMonitor(RequestShowEventArgs args)
        {
            this.Height = this.Height * args.NotificationCount;

            switch (args.NotificationPosition)
            {
                case Position.TopLeft:
                    this.Left = args.TargetMonitor.WorkingArea.TopLeft.X;
                    this.Top = args.TargetMonitor.WorkingArea.TopLeft.Y;
                    break;

                case Position.BottomLeft:
                    this.Left = args.TargetMonitor.WorkingArea.BottomLeft.X;
                    this.Top = args.TargetMonitor.WorkingArea.BottomLeft.Y - this.Height;
                    break;

                case Position.TopRight:
                    this.Left = args.TargetMonitor.WorkingArea.TopRight.X - this.Width;
                    this.Top = args.TargetMonitor.WorkingArea.TopRight.Y;
                    break;

                case Position.BottomRight:
                    this.Left = args.TargetMonitor.WorkingArea.BottomRight.X - this.Width;
                    this.Top = args.TargetMonitor.WorkingArea.BottomRight.Y - this.Height;
                    break;

                default:
                    throw new NotSupportedException();
            }
            Show();
        }
    }
}