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
using System.Collections.Generic;
using Paragon.Plugins.Notifications.Controls;
using System.Collections;

namespace Paragon.Plugins.Notifications.Views
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private readonly IntPtr handle;
        private IMonitor monitor;
        private Position notificationPosition;

        public NotificationWindow()
        {
            InitializeComponent();
            handle = new WindowInteropHelper(this).EnsureHandle();
        }

		//DES-11128
        public void ShowOnMonitor(RequestShowEventArgs args)
        {
            monitor = args.TargetMonitor;
            notificationPosition = args.NotificationPosition;


            Show();
        }

        public void MoveNotificationWindow()
        {
            switch (notificationPosition)
            {
                case Position.TopLeft:
                    this.Left = monitor.WorkingArea.TopLeft.X;
                    this.Top = monitor.WorkingArea.TopLeft.Y;
                    break;

                case Position.BottomLeft:
                    this.Left = monitor.WorkingArea.BottomLeft.X;
                    this.Top = monitor.WorkingArea.BottomLeft.Y - this.Height;
                    break;

                case Position.TopRight:
                    this.Left = monitor.WorkingArea.TopRight.X - this.Width;
                    this.Top = monitor.WorkingArea.TopRight.Y;
                    break;

                case Position.BottomRight:
                    this.Left = monitor.WorkingArea.BottomRight.X - this.Width;
                    this.Top = monitor.WorkingArea.BottomRight.Y - this.Height;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}