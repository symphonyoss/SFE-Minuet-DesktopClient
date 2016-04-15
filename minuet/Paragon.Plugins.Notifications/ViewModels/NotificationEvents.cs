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

using Paragon.Plugins.Notifications.Models;
using Paragon.Plugins.Notifications.Mvvm;

namespace Paragon.Plugins.Notifications.ViewModels
{
    public class NotificationEvents
    {
        public class ActivateNotifciationCentre : PubSubEvent<EmptyArgs>
        {
        }

        public class AddNotification : PubSubEvent<Notification>
        {
        }

        public class AddNotificationArgs
        {
            public Notification Notification { get; set; }
            public Position Position { get; set; }
            public int MonitorIndex { get; set; }
        }

        public class AddNotificationWithPosition : PubSubEvent<AddNotificationArgs>
        {
        }

        public class Clear : PubSubEvent<ClearNotificationArgs>
        {
        }

        public class ClearAll : PubSubEvent<EmptyArgs>
        {
        }

        public class ClearGroup : PubSubEvent<string>
        {
        }

        public class ClearNotificationArgs
        {
            public string NotificationId { get; set; }
        }

        public class ClickNotification : PubSubEvent<ClickNotificationArgs>
        {
        }

        public class ClickNotificationArgs
        {
            public Notification Notification { get; set; }
            public bool ByCloseButton { get; set; }
        }

        public class CloseNotification : PubSubEvent<CloseNotificationArgs>
        {
        }

        public class CloseNotificationArgs
        {
            public Notification Notification { get; set; }
            public RemovedBy RemovedBy { get; set; }
        }

        public class RefreshNotificationCentre : PubSubEvent<EmptyArgs>
        {
        }
    }
}