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

using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications
{
    public class NotificationWindowManager : INotificationWindowManager
    {
        private readonly IViewModelFactory viewModelFactory;

        private NotificationWindowViewModel notificationWindow;

        UserConfigurationWindowViewModel configurationWindow = null;

        public NotificationWindowManager(IViewModelFactory viewModelFactory)
        {
            this.viewModelFactory = viewModelFactory;
        }

        public NotificationWindowViewModel ShowNotificationWindow()
        {
            return notificationWindow ?? (notificationWindow = viewModelFactory.CreateNotificationWindow());
        }

        public UserConfigurationWindowViewModel ShowSettings(IApplicationWindow owner)
        {
            if (configurationWindow == null)
            {
                configurationWindow = viewModelFactory.CreateUserConfigurationWindow(owner);
                configurationWindow.RequestClose += configurationWindow_RequestClose;
            }
            configurationWindow.Show();

            return configurationWindow;
        }

        void configurationWindow_RequestClose(object sender, System.EventArgs e)
        {
            if (configurationWindow != null)
            {
                configurationWindow.RequestClose -= configurationWindow_RequestClose;
                configurationWindow = null;
            }
        }

        public void Shutdown()
        {
            if (notificationWindow != null)
            {
                notificationWindow.Close();
            }
        }
    }
}