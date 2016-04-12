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

using Paragon.Plugins.Notifications.Configuration;
using Paragon.Plugins.Notifications.Contracts;
using Paragon.Plugins.Notifications.Models;
using Paragon.Plugins.Notifications.Mvvm;
using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications.Client
{
    public class LocalServiceFactory
    {
        public LocalServiceFactory(INotificationSettings notificationSettings)
        {
            var monitors = new Monitors();

            EventAggregator = new EventAggregator();
            NotificationFactory = new NotificationFactory(() => new Notification(EventAggregator));

            var viewModelFactory = new ViewModelFactory(
                EventAggregator,
                () => new UserConfigurationWindowViewModel(
                    monitors,
                    notificationSettings,
                    EventAggregator,
                    NotificationFactory),
                () => new NotificationWindowViewModel(
                    monitors,
                    notificationSettings,
                    EventAggregator));

            NotificationWindowManager = new NotificationWindowManager(viewModelFactory);
            NotificationService = new NotificationService(() => NotificationWindowManager);
        }

        public IEventAggregator EventAggregator { get; private set; }

        public INotificationFactory NotificationFactory { get; private set; }

        public INotificationService NotificationService { get; private set; }

        public INotificationWindowManager NotificationWindowManager { get; private set; }
    }
}