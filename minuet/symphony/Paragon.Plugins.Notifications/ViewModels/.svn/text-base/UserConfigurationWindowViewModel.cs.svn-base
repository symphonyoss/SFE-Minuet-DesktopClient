using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Paragon.Plugins.Notifications.Configuration;
using Paragon.Plugins.Notifications.Contracts;
using Paragon.Plugins.Notifications.Models;
using Paragon.Plugins.Notifications.Mvvm;

namespace Paragon.Plugins.Notifications.ViewModels
{
    public class UserConfigurationWindowViewModel : BindableBase
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMonitors monitors;
        private readonly INotificationFactory notificationFactory;
        private readonly INotificationSettings notificationSettings;

        private int selectedMonitor;
        private Position selectedPosition;

        public UserConfigurationWindowViewModel(
            IMonitors monitors,
            INotificationSettings notificationSettings,
            IEventAggregator eventAggregator,
            INotificationFactory notificationFactory)
        {
            this.monitors = monitors;
            this.notificationSettings = notificationSettings;
            this.eventAggregator = eventAggregator;
            this.notificationFactory = notificationFactory;

            CancelCommand = new DelegateCommand(OnCancel);
            SaveCommand = new DelegateCommand(OnSave);
        }

        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        public ICollectionView AvailableMonitors { get; private set; }

        public int SelectedMonitor
        {
            get { return selectedMonitor; }
            set
            {
                if (value == selectedMonitor)
                {
                    return;
                }
                selectedMonitor = value;
                OnPropertyChanged("SelectedMonitor");
            }
        }

        public Position SelectedPosition
        {
            get { return selectedPosition; }
            set
            {
                if (value == selectedPosition)
                {
                    return;
                }
                selectedPosition = value;
                OnPropertyChanged("SelectedPosition");
            }
        }

        public event EventHandler RequestClose;
        public event EventHandler RequestShow;

        public void ClearSamples()
        {
            eventAggregator
                .GetEvent<NotificationEvents.ClearGroup>()
                .Publish("one");

            eventAggregator
                .GetEvent<NotificationEvents.ClearGroup>()
                .Publish("two");
        }

        public void Show()
        {
            var availableMonitors = monitors.AllMonitors().Count();
            var monitorIndexes = Enumerable.Range(0, availableMonitors).ToArray();
            var monitor = notificationSettings.GetSelectedMonitor();
            monitor = monitor < availableMonitors ? monitor : 0;

            AvailableMonitors = CollectionViewSource.GetDefaultView(monitorIndexes);
            AvailableMonitors.MoveCurrentTo(monitor);
            SelectedMonitor = monitor;

            SelectedPosition = notificationSettings.GetPosition();

            // subcribe to change events after rehydrating 
            // otherwise each property change will recreate samples
            PropertyChanged += (sender, args) =>
            {
                var propertyName = args.PropertyName;

                if (propertyName == "SelectedPosition"
                    || propertyName == "SelectedMonitor")
                {
                    ClearSamples();
                    ShowSample(SelectedPosition, SelectedMonitor);
                }
            };

            ShowSample(SelectedPosition, SelectedMonitor);

            var onRequestShow = RequestShow;
            if (onRequestShow != null)
            {
                onRequestShow(this, EventArgs.Empty);
            }
        }

        protected void OnRequestClose()
        {
            ClearSamples();

            var onRequestClose = RequestClose;
            onRequestClose(this, EventArgs.Empty);
        }

        private void OnCancel()
        {
            OnRequestClose();
        }

        private void OnSave()
        {
            notificationSettings.Save(SelectedMonitor, SelectedPosition);

            OnRequestClose();
        }

        private void ShowSample(Position position, int monitorIndex)
        {
            var notificationOneOptions = new NotificationOptions
            {
                BackgroundColor = "ff0000",
                Group = "one",
                IsPersistent = true,
                Message = "This is how an alert will look at this position.",
                Title = "First Sample Alert",
            };

            var notificationOne = notificationFactory.Create(notificationOneOptions);

            var notificationTwoOptions = new NotificationOptions
            {
                Group = "two",
                IsPersistent = true,
                Message = "Alert positioning affects the order.",
                Title = "Second Sample Alert",
            };

            var notificationTwo = notificationFactory.Create(notificationTwoOptions);

            eventAggregator
                .GetEvent<NotificationEvents.AddNotificationWithPosition>()
                .Publish(new NotificationEvents.AddNotificationArgs
                {
                    Notification = notificationOne,
                    Position = position,
                    MonitorIndex = monitorIndex
                });

            eventAggregator
                .GetEvent<NotificationEvents.AddNotificationWithPosition>()
                .Publish(new NotificationEvents.AddNotificationArgs
                {
                    Notification = notificationTwo,
                    Position = position,
                    MonitorIndex = monitorIndex
                });
        }
    }
}