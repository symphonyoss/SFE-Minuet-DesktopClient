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
using Paragon.Plugins.Notifications.Mvvm;
using Paragon.Plugins.Notifications.Views;

namespace Paragon.Plugins.Notifications.ViewModels
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly Func<NotificationWindowViewModel> createNotificationWindowViewModel;
        private readonly Func<UserConfigurationWindowViewModel> createUserConfigurationWindowViewModel;
        private readonly IEventAggregator eventAggregator;

        public ViewModelFactory(
            IEventAggregator eventAggregator,
            Func<UserConfigurationWindowViewModel> createUserConfigurationWindowViewModel,
            Func<NotificationWindowViewModel> createNotificationWindowViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.createUserConfigurationWindowViewModel = createUserConfigurationWindowViewModel;
            this.createNotificationWindowViewModel = createNotificationWindowViewModel;
        }

        public NotificationWindowViewModel CreateNotificationWindow()
        {
            var window = new NotificationWindow();
            var viewModel = createNotificationWindowViewModel();

            EventHandler<RequestShowEventArgs> requestShowHandler = (sender, args) => window.ShowOnMonitor(args.TargetMonitor);
            EventHandler requestCloseHandler = (sender, args) => window.Close();

            viewModel.RequestClose += requestCloseHandler;
            viewModel.RequestShow += requestShowHandler;

            window.DataContext = viewModel;
            window.Closed += (sender, args) =>
            {
                viewModel.RequestClose -= requestCloseHandler;
                viewModel.RequestShow -= requestShowHandler;
            };

            return viewModel;
        }

        public UserConfigurationWindowViewModel CreateUserConfigurationWindow(IApplicationWindow owner)
        {
            var window = new UserConfigurationWindow();
            window.Owner = owner as Window;

            var viewModel = createUserConfigurationWindowViewModel();

            EventHandler requestCloseHandler = (sender, args) => window.Close();
            EventHandler requestShowHandler = (sender, args) =>
            {
                window.Show();
                window.Focus();
            };

            viewModel.RequestClose += requestCloseHandler;
            viewModel.RequestShow += requestShowHandler;

            window.DataContext = viewModel;
            window.Closed += (sender, args) =>
            {
                viewModel.RequestClose -= requestCloseHandler;
                viewModel.RequestShow -= requestShowHandler;
                viewModel.ClearSamples();

                var refreshNotificationCentre = eventAggregator.GetEvent<NotificationEvents.RefreshNotificationCentre>();
                refreshNotificationCentre.Publish(EmptyArgs.Empty);
            };

            return viewModel;
        }
    }
}