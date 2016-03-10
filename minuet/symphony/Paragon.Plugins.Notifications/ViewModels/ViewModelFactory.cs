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