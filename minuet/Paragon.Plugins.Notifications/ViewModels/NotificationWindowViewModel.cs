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
using System.Collections.Generic;
using System.Linq;
using Paragon.Plugins.Notifications.Configuration;
using Paragon.Plugins.Notifications.Models;
using Paragon.Plugins.Notifications.Mvvm;

namespace Paragon.Plugins.Notifications.ViewModels
{
    public class NotificationWindowViewModel : BindableBase
    {
        private readonly NotificationCollection bottomLeftNotifications;
        private readonly NotificationCollection bottomRightNotifications;
        private readonly NotificationCollection topLeftNotifications;
        private readonly NotificationCollection topRightNotifications;

        private readonly IEventAggregator eventAggregator;
        private readonly IMonitors monitors;
        private readonly INotificationSettings notificationSettings;

        public NotificationWindowViewModel(
            IMonitors monitors,
            INotificationSettings notificationSettings,
            IEventAggregator eventAggregator)
        {
            this.notificationSettings = notificationSettings;
            this.eventAggregator = eventAggregator;
            this.monitors = monitors;

            topLeftNotifications = new NotificationCollection();
            topLeftNotifications.Removed += OnRemoved;

            topRightNotifications = new NotificationCollection();
            topRightNotifications.Removed += OnRemoved;

            bottomLeftNotifications = new NotificationCollection();
            bottomLeftNotifications.Removed += OnRemoved;

            bottomRightNotifications = new NotificationCollection();
            bottomRightNotifications.Removed += OnRemoved;

            eventAggregator
                .GetEvent<NotificationEvents.AddNotification>()
                .Subscribe(OnAdd);

            eventAggregator
                .GetEvent<NotificationEvents.AddNotificationWithPosition>()
                .Subscribe(OnAdd);

            eventAggregator
                .GetEvent<NotificationEvents.ClearAll>()
                .Subscribe(OnClearAll);

            eventAggregator
                .GetEvent<NotificationEvents.ClearGroup>()
                .Subscribe(OnClearGroup);

            eventAggregator
                .GetEvent<NotificationEvents.ClickNotification>()
                .Subscribe(OnClick);

            eventAggregator
                .GetEvent<NotificationEvents.RefreshNotificationCentre>()
                .Subscribe(OnRefresh);
        }

        public IList<Notification> TopLeftNotifications
        {
            get { return topLeftNotifications.Items; }
        }

        public IList<Notification> TopRightNotifications
        {
            get { return topRightNotifications.Items; }
        }

        public IList<Notification> BottomLeftNotifications
        {
            get { return bottomLeftNotifications.Items; }
        }

        public IList<Notification> BottomRightNotifications
        {
            get { return bottomRightNotifications.Items; }
        }

        public event EventHandler RequestClose;
        public event EventHandler RequestHide;
        public event EventHandler<RequestShowEventArgs> RequestShow;

        public void Add(Notification notification)
        {
            OnAdd(notification);
        }

        public void ClearAll()
        {
            OnClearAll(EmptyArgs.Empty);
        }

        public void Close()
        {
            OnRequestClose();
        }

        public void ClearGroup(string group)
        {
            OnClearGroup(group);
        }

        public void Show()
        {
            OnRequestShow();
        }
		
		//DES-11128
        public void OnRequestHide()
        {
            var onRequestHide = RequestHide;
            if (onRequestHide != null)
            {
                onRequestHide(this, EventArgs.Empty);
            }            
        }

        protected void OnRequestClose()
        {
            eventAggregator
                .GetEvent<NotificationEvents.AddNotification>()
                .Unsubscribe(OnAdd);

            eventAggregator
                .GetEvent<NotificationEvents.AddNotificationWithPosition>()
                .Unsubscribe(OnAdd);

            eventAggregator
                .GetEvent<NotificationEvents.ClearAll>()
                .Unsubscribe(OnClearAll);

            eventAggregator
                .GetEvent<NotificationEvents.ClearGroup>()
                .Unsubscribe(OnClearGroup);

            eventAggregator
                .GetEvent<NotificationEvents.ClickNotification>()
                .Unsubscribe(OnClick);

            eventAggregator
                .GetEvent<NotificationEvents.RefreshNotificationCentre>()
                .Unsubscribe(OnRefresh);

            var onRequestClose = RequestClose;
            if (onRequestClose != null)
            {
                onRequestClose(this, EventArgs.Empty);
            }
        }

        protected void OnRequestShow()
        {
            var targetMonitor = GetTargetMonitor();
            //DES-11128
			OnRequestShow(new RequestShowEventArgs { TargetMonitor = targetMonitor });
        }
		
		//DES-11128
        protected void OnRequestShow(RequestShowEventArgs args)
        {
            var onRequestActivate = RequestShow;
            if (onRequestActivate != null)
            {
                onRequestActivate(this, args);
            }
        }

        private void AddNotification(Notification notification)
        {
            var collection = GetNotificationCollection(notification.Position);
            collection.Add(notification);
        }

        private NotificationCollection GetNotificationCollection(Position position)
        {
            switch (position)
            {
                case Position.TopLeft:
                    return topLeftNotifications;

                case Position.TopRight:
                    return topRightNotifications;

                case Position.BottomLeft:
                    return bottomLeftNotifications;

                case Position.BottomRight:
                    return bottomRightNotifications;

                default:
                    throw new NotSupportedException();
            }
        }

        private IMonitor GetTargetMonitor()
        {
            var targetMonitorIndex = notificationSettings.GetSelectedMonitor();

            return GetTargetMonitor(targetMonitorIndex);
        }

        private IMonitor GetTargetMonitor(int targetMonitorIndex)
        {
            var allMonitors = monitors.AllMonitors().ToArray();
            var targetMonitor = targetMonitorIndex < allMonitors.Length
                ? allMonitors[targetMonitorIndex]
                : allMonitors.First();

            return targetMonitor;
        }

        private void OnAdd(Notification notification)
        {
            var position = notificationSettings.GetPosition();
            var targetMonitor = GetTargetMonitor();

            OnAdd(notification, position, targetMonitor);
        }

        private void OnAdd(NotificationEvents.AddNotificationArgs args)
        {
            var targetMonitor = GetTargetMonitor(args.MonitorIndex);

            OnAdd(args.Notification, args.Position, targetMonitor);
        }

        private void OnAdd(Notification notification, Position position, IMonitor monitor)
        {
            notification.Position = position;

            AddNotification(notification);
            
			//DES-11128
            OnRequestShow(new RequestShowEventArgs {TargetMonitor = monitor, NotificationCount = GetNotificationCollection(position).Items.Count, NotificationPosition = position});
        }

        private void OnClearAll(EmptyArgs args)
        {
            UpdateNotificationCollection((collection, a) => collection.Clear(), args);
        }

        private void OnClearGroup(string group)
        {
            UpdateNotificationCollection((collection, arg) => collection.RemoveGroup(arg), group);
        }

        private void OnClick(NotificationEvents.ClickNotificationArgs clickNotificationArgs)
        {
            UpdateNotificationCollection(
                (collection, arg) => collection.Remove(arg, RemovedBy.User),
                clickNotificationArgs.Notification);
        }

        private void OnRemoved(object sender, NotificationRemovedArgs removedArgs)
        {
            eventAggregator
                .GetEvent<NotificationEvents.CloseNotification>()
                .Publish(new NotificationEvents.CloseNotificationArgs {RemovedBy = removedArgs.RemovedBy, Notification = removedArgs.Notification});
			//DES-11128
            if (topLeftNotifications.Items.Count == 0 && 
                topRightNotifications.Items.Count == 0 && 
                bottomLeftNotifications.Items.Count == 0 &&
                bottomRightNotifications.Items.Count == 0)
            {
                OnRequestHide();
            }
        }

        private void OnRefresh(EmptyArgs args)
        {
            OnRequestShow();
        }

        private void UpdateNotificationCollection<TArg>(Action<NotificationCollection, TArg> action, TArg args)
        {
            action(topLeftNotifications, args);
            action(topRightNotifications, args);
            action(bottomLeftNotifications, args);
            action(bottomRightNotifications, args);
        }
    }
}