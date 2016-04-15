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
using System.Linq;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paragon.Plugins.MessageBus;
using Paragon.Plugins.Notifications.Annotations;
using Paragon.Plugins.Notifications.Client;
using Paragon.Plugins.Notifications.Configuration;
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications
{
    [JavaScriptPlugin(Name = "paragon.notifications", IsBrowserSide = true)]
    public class ParagonNotificationPlugin : ParagonPlugin
    {
        private readonly Dispatcher _dispatcher;
        private readonly NotificationClient _notificationClient;
        private readonly INotificationService _notificationService;
        private IMessageBusPlugin _messageBusplugin;
        private Settings _settings;
        private bool _dnd;
        private const string DndTopic = "com.gs.dnd";

        public ParagonNotificationPlugin()
        {
            _dispatcher = System.Windows.Application.Current.Dispatcher;
            var nsettings = new NotificationSettings(this);
            var serviceFactory = new LocalServiceFactory(nsettings);
            _notificationClient = new NotificationClient(serviceFactory);
            _notificationClient.Clicked += NotificationClientOnClicked;
            _notificationClient.Closed += NotificationClientOnClosed;
            _notificationService = new LocalNotificationService(serviceFactory, System.Windows.Application.Current.Dispatcher);
        }

        [JavaScriptPluginMember(Name = "onSettingsChanged"), UsedImplicitly]
        public event JavaScriptPluginCallback SettingsChanged;

        [JavaScriptPluginMember(Name = "onClicked"), UsedImplicitly]
        public event JavaScriptPluginCallback Clicked;

        [JavaScriptPluginMember(Name = "onClosed"), UsedImplicitly]
        public event JavaScriptPluginCallback Closed;

        [JavaScriptPluginMember(Name = "onShowSettings"), UsedImplicitly]
        public event JavaScriptPluginCallback ShowSettings;

        [JavaScriptPluginMember, UsedImplicitly]
        public void SetSettings(Settings settings)
        {
            _settings = settings;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void ClearAll()
        {
            _dispatcher.Invoke(new Action(() => _notificationClient.ClearAll()));
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void ClearGroup(string group)
        {
            _dispatcher.Invoke(new Action(() => _notificationClient.ClearGroup(@group)));
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public string Create(NotificationOptions options)
        {
            if (_dnd)
            {
                return null;
            }
            return _dispatcher.Invoke(new Func<string>(() => _notificationClient.Create(options))) as string;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void OpenSettings()
        {
            var owner = Application.FindWindow(PluginExecutionContext.BrowserIdentifier);
            _dispatcher.Invoke(new Action(() => _notificationClient.ShowSettings(owner)));

            var evnt = ShowSettings;
            if (evnt != null)
            {
                evnt();
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _messageBusplugin = Application.Plugins.OfType<MessageBusPlugin>().FirstOrDefault();

            if (_messageBusplugin != null)
            {
                SubscribeToMessageBus();
            }

            _notificationService.Start();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            if (_messageBusplugin != null)
            {
                _messageBusplugin.Unsubscribe(DndTopic, String.Empty);
            }
            var service = _notificationService;
            if (service != null)
            {
                service.Shutdown();
            }
        }

        private void NotificationClientOnClicked(object sender, ClickNotificationEventArgs e)
        {
            var evnt = Clicked;
            if (evnt != null)
            {
                evnt(e.NotificationId);
            }
        }

        private void NotificationClientOnClosed(object sender, CloseNotificationEventArgs e)
        {
            var evnt = Closed;
            if (evnt != null)
            {
                evnt(e.NotificationId);
            }
        }

        private void SetDnd(bool dndValue)
        {
            if (_settings == null)
            {
                return;
            }
            _dnd = dndValue;
        }

        private void SubscribeToMessageBus()
        {
            _messageBusplugin.Subscribe(DndTopic, String.Empty);
            _messageBusplugin.OnMessage += args =>
            {
                if (args != null && args.Length > 1 && args[0] != null &&
                    args[0].ToString().Equals(DndTopic, StringComparison.InvariantCultureIgnoreCase))
                {
                    var message = args[1] as Message;
                    if (message != null)
                    {
                        var data = (JObject)JsonConvert.DeserializeObject(message.Data.ToString());
                        var operation = data.GetValue("operation").ToString();
                        if (operation == "dnd")
                        {
                            var status = (bool)data.GetValue("status");
                            SetDnd(status);
                        }
                    }
                }
            };
        }


        public class NotificationSettings : INotificationSettings
        {
            private readonly ParagonNotificationPlugin _parent;

            public NotificationSettings(ParagonNotificationPlugin parent)
            {
                _parent = parent;
            }

            public int GetSelectedMonitor()
            {
                return _parent._settings.SelectedMonitor;
            }

            public Position GetPosition()
            {
                return (Position) Enum.Parse(typeof (Position), _parent._settings.Position);
            }

            public bool GetDnd()
            {
                return _parent._dnd;
            }

            public void Save(int selectedMonitor, Position position, bool dnd)
            {
                _parent._settings.SelectedMonitor = selectedMonitor;
                _parent._settings.Position = position.ToString();
                _parent._dnd = dnd;

                var changedEvent = _parent.SettingsChanged;
                if (changedEvent != null)
                {
                    changedEvent(_parent._settings);
                }
            }
        }

        public class Settings
        {
            public int SelectedMonitor { get; set; }
            public string Position { get; set; }
        }
    }
}