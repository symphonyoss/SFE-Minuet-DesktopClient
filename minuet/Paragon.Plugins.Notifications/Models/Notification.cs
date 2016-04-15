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

using System.Windows.Media;
using Paragon.Plugins.Notifications.Mvvm;
using Paragon.Plugins.Notifications.ViewModels;

namespace Paragon.Plugins.Notifications.Models
{
    public class Notification : BindableBase
    {
        private readonly IEventAggregator eventAggregator;

        private SolidColorBrush backgroundColor;
        private SolidColorBrush blinkColor;
        private string callback;
        private string callbackArg;
        private bool canBlink;
        private bool canPlaySound;
        private double eventTime;
        private string group;
        private string iconUrl;
        private bool isClickable;
        private bool isPersistent;
        private bool isRemoving;
        private string message;
        private string notificationId;
        private Position position;
        private string soundFile;
        private string title;

        public Notification(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            ClickCommand = new DelegateCommand(OnClick);
            CloseCommand = new DelegateCommand(OnClose);
        }

        public DelegateCommand ButtonClickCommand { get; private set; }
        public DelegateCommand ClickCommand { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }

        public SolidColorBrush BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value.Equals(backgroundColor))
                {
                    return;
                }
                backgroundColor = value;
                OnPropertyChanged("BackgroundColor");
            }
        }

        public SolidColorBrush BlinkColor
        {
            get { return blinkColor; }
            set
            {
                if (value.Equals(blinkColor))
                {
                    return;
                }
                blinkColor = value;
                OnPropertyChanged("BlinkColor");
            }
        }

        public string Callback
        {
            get { return callback; }
            set
            {
                if (value == callback)
                {
                    return;
                }
                callback = value;
                OnPropertyChanged("Callback");
            }
        }

        public string CallbackArg
        {
            get { return callbackArg; }
            set
            {
                if (value == callbackArg)
                {
                    return;
                }
                callbackArg = value;
                OnPropertyChanged("CallbackArg");
            }
        }

        public bool CanBlink
        {
            get { return canBlink; }
            set
            {
                if (value.Equals(canBlink))
                {
                    return;
                }
                canBlink = value;
                OnPropertyChanged("CanBlink");
            }
        }

        public bool CanPlaySound
        {
            get { return canPlaySound; }
            set
            {
                if (value.Equals(canPlaySound))
                {
                    return;
                }
                canPlaySound = value;
                OnPropertyChanged("CanPlaySound");
            }
        }

        public double EventTime
        {
            get { return eventTime; }
            set
            {
                if (value.Equals(eventTime))
                {
                    return;
                }
                eventTime = value;
                OnPropertyChanged("EventTime");
            }
        }

        public string Group
        {
            get { return @group; }
            set
            {
                if (value == @group)
                {
                    return;
                }
                @group = value;
                OnPropertyChanged("Group");
            }
        }

        public string IconUrl
        {
            get { return iconUrl; }
            set
            {
                if (value == iconUrl)
                {
                    return;
                }
                iconUrl = value;
                OnPropertyChanged("IconUrl");
            }
        }

        public bool IsClickable
        {
            get { return isClickable; }
            set
            {
                if (value.Equals(isClickable))
                {
                    return;
                }
                isClickable = value;
                OnPropertyChanged("IsClickable");
            }
        }

        public bool IsPersistent
        {
            get { return isPersistent; }
            set
            {
                if (value.Equals(isPersistent))
                {
                    return;
                }
                isPersistent = value;
                OnPropertyChanged("IsPersistent");
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                if (value == message)
                {
                    return;
                }
                message = value;
                OnPropertyChanged("Message");
            }
        }

        public string NotificationId
        {
            get { return notificationId; }
            set
            {
                if (value == notificationId)
                {
                    return;
                }
                notificationId = value;
                OnPropertyChanged("NotificationId");
            }
        }

        public string SoundFile
        {
            get { return soundFile; }
            set
            {
                if (value == soundFile)
                {
                    return;
                }
                soundFile = value;
                OnPropertyChanged("SoundFile");
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (value == title)
                {
                    return;
                }
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public bool IsRemoving
        {
            get { return isRemoving; }
            set
            {
                if (value == isRemoving)
                {
                    return;
                }
                isRemoving = value;
                OnPropertyChanged("IsRemoving");
            }
        }

        public Position Position
        {
            get { return position; }
            set
            {
                if (value == position)
                {
                    return;
                }
                position = value;
                OnPropertyChanged("Position");
            }
        }

        private void OnClick()
        {
            eventAggregator
                .GetEvent<NotificationEvents.ClickNotification>()
                .Publish(new NotificationEvents.ClickNotificationArgs {ByCloseButton = false, Notification = this});
        }

        private void OnClose()
        {
            eventAggregator
                .GetEvent<NotificationEvents.ClickNotification>()
                .Publish(new NotificationEvents.ClickNotificationArgs {ByCloseButton = true, Notification = this});
        }
    }
}