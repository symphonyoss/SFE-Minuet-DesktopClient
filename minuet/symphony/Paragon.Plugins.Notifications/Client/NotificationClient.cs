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
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications.Client
{
    public class NotificationClient : INotifications
    {
        private readonly INotifications notifications;
        private readonly INotificationWindowManager windowManager;

        public NotificationClient(LocalServiceFactory serviceFactory)
        {
            notifications = new Notifications(
                serviceFactory.EventAggregator,
                serviceFactory.NotificationFactory);

            windowManager = serviceFactory.NotificationWindowManager;
        }

        public event EventHandler<CloseNotificationEventArgs> Closed
        {
            add { notifications.Closed += value; }
            remove { notifications.Closed -= value; }
        }

        public event EventHandler<ClickNotificationEventArgs> Clicked
        {
            add { notifications.Clicked += value; }
            remove { notifications.Clicked -= value; }
        }

        public void ClearAll()
        {
            notifications.ClearAll();
        }

        public void ClearGroup(string group)
        {
            notifications.ClearGroup(group);
        }

        public string Create(NotificationOptions options)
        {
            return notifications.Create(options);
        }

        public string Create(string notificationId, NotificationOptions options)
        {
            return notifications.Create(notificationId, options);
        }

        public void ShowSettings(IApplicationWindow owner)
        {
            windowManager.ShowSettings(owner);
        }
    }
}