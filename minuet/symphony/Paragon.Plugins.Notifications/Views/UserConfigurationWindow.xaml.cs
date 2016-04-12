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
using System.Windows;

namespace Paragon.Plugins.Notifications.Views
{
    /// <summary>
    /// Interaction logic for ToastNotificationConfigurationWindow.xaml
    /// </summary>
    public partial class UserConfigurationWindow : Window
    {
        public UserConfigurationWindow()
        {
            InitializeComponent();

            this.Closed += UserConfigurationWindow_Closed;
        }

        void UserConfigurationWindow_Closed(object sender, System.EventArgs e)
        {
            if (this.DataContext is UserConfigurationWindowViewModel)
            {
                UserConfigurationWindowViewModel viewModel = this.DataContext as UserConfigurationWindowViewModel;
                if (viewModel.CancelCommand.CanExecute())
                    viewModel.CancelCommand.Execute();
            }
        }
    }
}