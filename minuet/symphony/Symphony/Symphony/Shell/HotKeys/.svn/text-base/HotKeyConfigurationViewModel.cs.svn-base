using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Symphony.Mvvm;

namespace Symphony.Shell.HotKeys
{
    public class HotKeyConfigurationViewModel : BindableBase
    {
        private readonly IHotKeySettings settings;
        private readonly IEventAggregator eventAggregator;
        private bool isHotKeyEnabled;
        private ModifierKeys selectedModifier;
        private Keys keys;

        public HotKeyConfigurationViewModel(
            IHotKeySettings settings,
            IEventAggregator eventAggregator)
        {
            this.settings = settings;
            this.eventAggregator = eventAggregator;

            var modifiers = new List<ModifierKeys>();
            modifiers.Add(ModifierKeys.Control);
            modifiers.Add(ModifierKeys.Alt);
            modifiers.Add(ModifierKeys.Control | ModifierKeys.Shift);
            modifiers.Add(ModifierKeys.Alt | ModifierKeys.Shift);
            modifiers.Add(ModifierKeys.Control | ModifierKeys.Alt);

            this.Modifiers = CollectionViewSource.GetDefaultView(modifiers);
            
            this.CancelCommand = new DelegateCommand(this.OnRequestClose);
            this.SaveCommand = new DelegateCommand(this.OnSave);
        }

        public event EventHandler RequestClose;
        public event EventHandler RequestShow;

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

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

        public void Show()
        {
            var modifier = this.settings.GetModifier();

            this.IsHotKeyEnabled = this.settings.GetIsHotKeyEnabled();
            this.Modifiers.MoveCurrentTo(modifier);
            this.SelectedModifier = modifier;

            this.Keys = this.settings.GetKeys();

            this.OnRequestShow();
        }

        protected virtual void OnRequestClose()
        {
            var onRequestClose = this.RequestClose;
            if (onRequestClose != null) onRequestClose(this, EventArgs.Empty);
        }

        protected virtual void OnRequestShow()
        {
            var onRequestShow = this.RequestShow;
            if (onRequestShow != null) onRequestShow(this, EventArgs.Empty);
        }

        private void OnSave()
        {
            this.settings.Save(
                this.IsHotKeyEnabled, 
                this.SelectedModifier, 
                this.Keys);

            this.eventAggregator
                .GetEvent<HotKeyEvents.SaveHotKey>()
                .Publish(new HotKeyEvents.SaveGetFocusHotKeyArgs
                {
                    IsEnabled = this.IsHotKeyEnabled,
                    Modifiers = this.SelectedModifier,
                    Keys = this.Keys,
                });

            this.OnRequestClose();
        }
    }
}
