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