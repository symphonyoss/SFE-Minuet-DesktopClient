using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace Symphony.Plugins.Hotkeys
{
    public class RequestSaveEventArgs: EventArgs
    {
        public RequestSaveEventArgs(bool isHotKeyEnabled, ModifierKeys selectedModifier, Keys keys)
        {
            this.isHotKeyEnabled = isHotKeyEnabled;
            this.selectedModifier = selectedModifier;
            this.keys = keys;
        }

        public bool isHotKeyEnabled { get; private set; }
        public ModifierKeys selectedModifier { get; private set; }
        public Keys keys { get; private set; }
    };


    public class HotKeyConfigurationViewModel : INotifyPropertyChanged
    {
        private bool isHotKeyEnabled;
        private ModifierKeys selectedModifier;
        private Keys keys;

        public HotKeyConfigurationViewModel()
        {

            var modifiers = new List<ModifierKeys>();
            modifiers.Add(ModifierKeys.Control);
            modifiers.Add(ModifierKeys.Alt);
            modifiers.Add(ModifierKeys.Control | ModifierKeys.Shift);
            modifiers.Add(ModifierKeys.Alt | ModifierKeys.Shift);
            modifiers.Add(ModifierKeys.Control | ModifierKeys.Alt);

            this.Modifiers = CollectionViewSource.GetDefaultView(modifiers);
            
            this.CancelCommand = new CommandHandler(this.OnRequestClose);
            this.SaveCommand = new CommandHandler(this.OnRequestSave);
        }

        public event EventHandler<RequestSaveEventArgs> RequestSave;
        public event EventHandler RequestClose;

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public bool IsHotKeyEnabled
        {
            get { return this.isHotKeyEnabled; }
            set
            {
                if (value == this.isHotKeyEnabled) return;
                this.isHotKeyEnabled = value;
                this.OnPropertyChanged("IsHotKeyEnabled");
            }
        }

        public ICollectionView Modifiers
        {
            get; private set;
        }

        public ModifierKeys SelectedModifier
        {
            get { return this.selectedModifier; }
            set
            {
                if (value == this.selectedModifier) return;
                this.selectedModifier = value;
                this.OnPropertyChanged("SelectedModifier");
            }
        }

        public Keys Keys
        {
            get { return this.keys; }
            set
            {
                if (value == this.keys) return;
                this.keys = value;
                this.OnPropertyChanged("Keys");
            }
        }

        protected virtual void OnRequestSave()
        {
            var onRequestSave = this.RequestSave;
            if (onRequestSave != null) onRequestSave(this, new RequestSaveEventArgs(this.IsHotKeyEnabled, this.SelectedModifier, this.Keys));
        }

        protected virtual void OnRequestClose()
        {
            var onRequestClose = this.RequestClose;
            if (onRequestClose != null) onRequestClose(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private class CommandHandler : ICommand
        {
            private Action _action;
            public CommandHandler(Action action)
            {
                this._action = action;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                this._action();
            }
        }
    }
}
