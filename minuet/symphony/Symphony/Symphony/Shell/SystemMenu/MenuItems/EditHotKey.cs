using System;
using Paragon.Plugins;
using Symphony.Mvvm;
using Symphony.Shell.HotKeys;

namespace Symphony.Shell.SystemMenu.MenuItems
{
    public class EditHotKey : SystemMenuItem
    {
        private readonly IApplicationWindow applicationWindow;
        private readonly IHotKeySettings settings;
        private readonly IEventAggregator eventAggregator;

        public EditHotKey(
            IApplicationWindow applicationWindow, 
            IHotKeySettings settings,
            IEventAggregator eventAggregator)
        {
            this.applicationWindow = applicationWindow;
            this.settings = settings;
            this.eventAggregator = eventAggregator;

            this.Id = 104;
            this.Header = "Edit Shortcuts";
            this.Command = new DelegateCommand(this.OnEdit);
        }

        private void OnEdit()
        {
            Action onDispatcher = this.CreateAndShowView;

            var window = this.applicationWindow.Unwrap();
            window.Dispatcher.Invoke(onDispatcher);
        }

        private void CreateAndShowView()
        {
            var hotKeyWindow = new HotKeyConfigurationWindow();
            var viewModel = new HotKeyConfigurationViewModel(this.settings, this.eventAggregator);

            EventHandler requestShowHandler = (sender, args) =>
            {
                hotKeyWindow.Owner = this.applicationWindow.Unwrap();
                hotKeyWindow.Show();
            };
            EventHandler requestCloseHandler = (sender, args) => hotKeyWindow.Close();

            viewModel.RequestClose += requestCloseHandler;
            viewModel.RequestShow += requestShowHandler;

            hotKeyWindow.DataContext = viewModel;
            hotKeyWindow.Closing += (sender, args) =>
            {
                viewModel.RequestClose -= requestCloseHandler;
                viewModel.RequestShow -= requestShowHandler;
            };

            viewModel.Show();
        }
    }
}
