using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Paragon.Plugins;
using Symphony.Configuration;
using Symphony.Plugins.Hotkeys;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony.settings", IsBrowserSide = true)]
    public class SettingsPlugin: IParagonPlugin
    {
        private LegacySymphonySettings settings;
        private IApplication application;

        [JavaScriptPluginMember(Name = "onHotKeysEdited")]
        public event JavaScriptPluginCallback HotKeysEdited;

        public void Initialize(IApplication application)
        {
            this.application = application;
        }

        public void Shutdown()
        {
        }

        [JavaScriptPluginMember]
        public LegacySymphonySettings GetLegacy()
        {
            if (this.settings == null)
            {
                this.settings = new LegacySymphonySettings();
            }
            settings.Load();

            return settings;
        }

        [JavaScriptPluginMember]
        public void ShowEditHotKeysDialog(EditHotKeyDialogArgs dialogArgs)
        {
            var mainWindow = (Window)this.application.WindowManager.AllWindows[0];

            var showDialog = new Action(() =>
            {
                var viewModel = new HotKeyConfigurationViewModel();
                var window = new HotKeyConfigurationWindow();

                EventHandler<RequestSaveEventArgs> requestSaveHandler = (sender, args) =>
                {
                    JavaScriptPluginCallback handler = HotKeysEdited;
                    if (handler != null)
                    {
                        handler(new object[] { args.isHotKeyEnabled, args.selectedModifier, args.keys });
                    }
                    window.Close();
                };
                EventHandler requestCloseHandler = (sender, args) => window.Close();

                viewModel.RequestSave += requestSaveHandler;
                viewModel.RequestClose += requestCloseHandler;

                viewModel.IsHotKeyEnabled = dialogArgs.IsEnabled;
                viewModel.SelectedModifier = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), dialogArgs.Modifier);
                viewModel.Keys = (Keys)Enum.Parse(typeof(Keys), dialogArgs.Key);

                window.DataContext = viewModel;
                window.Closing += (sender, args) =>
                {
                    viewModel.RequestSave -= requestSaveHandler;
                    viewModel.RequestClose -= requestCloseHandler;
                };

                window.Owner = mainWindow;
                window.ShowDialog();
            });

            if (!mainWindow.Dispatcher.CheckAccess())
            {
                mainWindow.Dispatcher.Invoke(showDialog);
            }
            else
            {
                showDialog.Invoke();
            }
        }
    }

    public class EditHotKeyDialogArgs
    {
        public bool IsEnabled { get; set; }
        public string Modifier { get; set; }
        public string Key { get; set; }
    }
}
