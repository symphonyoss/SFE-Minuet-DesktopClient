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
using System.Windows.Threading;
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly Dispatcher dispatcher;
        private readonly Func<INotificationWindowManager> getNotificationWindowManager;

        public NotificationService(
            Func<INotificationWindowManager> getNotificationWindowManager)
            : this(getNotificationWindowManager, Application.Current.Dispatcher)
        {
        }

        public NotificationService(
            Func<INotificationWindowManager> getNotificationWindowManager,
            Dispatcher dispatcher)
        {
            this.getNotificationWindowManager = getNotificationWindowManager;
            this.dispatcher = dispatcher;
        }

        public void Start()
        {
            ShowNotificationWindow();
        }

        public void Shutdown()
        {
            ShutdownNotificationWindow();
        }

        private void ShowNotificationWindow()
        {
            var notificationWindowManager = getNotificationWindowManager();
            Action showAction = () => notificationWindowManager.ShowNotificationWindow();

            dispatcher.Invoke(showAction);
        }

        private void ShutdownNotificationWindow()
        {
            var notificationWindowManager = getNotificationWindowManager();
            Action shutdownAction = notificationWindowManager.Shutdown;

            dispatcher.Invoke(shutdownAction);
        }
    }
}