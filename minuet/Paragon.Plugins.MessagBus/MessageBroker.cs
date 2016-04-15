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

using Newtonsoft.Json;
using System;
using System.Threading;
using WebSocket4Net;

namespace Paragon.Plugins.MessageBus
{
    public class MessageBroker : IMessageBroker
    {
        public const int DefaultPort = 65534;
        private static int _idcount = 1;
        private readonly TimeSpan _reconnectFrequency = TimeSpan.FromSeconds(10);
        private long _disconnectRequested;
        private readonly ILogger _logger;
        private WebSocket _socket;
        private MessageBrokerWatcher _watcher;
        public event Action Connected;
        public event Action Disconnected;
        public event Action<Exception> Error;
        public event Action<string, object> MessageReceived;
        private int _brokerPort = DefaultPort;

        public MessageBroker(ILogger logger, MessageBrokerWatcher watcher = null, int brokerPort = DefaultPort)
        {
            _logger = logger;
            _watcher = watcher;
            _brokerPort = brokerPort;
        }

        public void Connect()
        {
            if (_watcher == null)
            {
                _watcher = CreateBrokerWatcher();
            }

            _watcher.Start(OnBrokerProcessStarted);
        }

        public void Disconnect()
        {
            Interlocked.Exchange(ref _disconnectRequested, 1);
            DisconnectInternal();
            if (_watcher != null)
            {
                _watcher.Stop();
                _watcher = null;
            }
        }

        public string GetState(string topic)
        {
            var state = WebSocketState.None.ToString();
            if (_socket != null)
            {
                state = _socket.State.ToString();
            }

            return state;
        }

        public void PublishMessage(string topic, object msg)
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                var payload = new Message { Type = "publish", Topic = topic, Data = msg };
                var strPayload = JsonConvert.SerializeObject(payload);
                _socket.Send(strPayload);
            }
        }

        public void SendMessage(string address, object msg, string responseId)
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                var payload = new Message {Type = "send", Topic = address, Rid = responseId, Data = msg};
                var strPayload = JsonConvert.SerializeObject(payload);
                _socket.Send(strPayload);
            }
        }

        public void Subscribe(string topic, string responseId)
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                var payload = new Message {Type = "register", Topic = topic, Rid = responseId};
                var strPayload = JsonConvert.SerializeObject(payload);
                _socket.Send(strPayload);
            }
        }

        public void Unsubscribe(string topic, string responseId)
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                var payload = new Message {Type = "unregister", Topic = topic, Rid = responseId};
                var strPayload = JsonConvert.SerializeObject(payload);
                _socket.Send(strPayload);
            }
        }

        private void Reconnect()
        {
            try
            {
                if (Interlocked.Read(ref _disconnectRequested) == 0)
                {
                    Thread.Sleep(_reconnectFrequency);
                    _logger.Warn("Attempting to re-connect to the broker.");
                    DisconnectInternal();
                    ConnectInternal();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to reconnect.", ex);
            }
        }

        private void OnBrokerProcessStarted()
        {
            ConnectInternal();
        }

        private void ConnectInternal()
        {
            if (_socket != null)
            {
                return;
            }

            _socket = new WebSocket(string.Format("ws://localhost:{0}?appid=app{1}", _brokerPort, _idcount++));
            _socket.Opened += OnSocketOpened;
            _socket.Error += OnSocketError;
            _socket.Closed += OnSocketClosed;
            _socket.MessageReceived += OnSocketMessageReceived;
            _socket.Open();
        }

        private void DisconnectInternal()
        {
            if (_socket == null)
            {
                return;
            }

            _socket.Opened -= OnSocketOpened;
            _socket.Error -= OnSocketError;
            _socket.Closed -= OnSocketClosed;
            _socket.MessageReceived -= OnSocketMessageReceived;

            if (_socket.State == WebSocketState.Open ||
                _socket.State == WebSocketState.Connecting)
            {
                try
                {
                    _socket.Close();
                }
                catch (Exception e)
                {
                    _logger.Warn("Error disconnection from message broker: " + e.Message);
                }
            }

            _socket = null;
        }

        private void OnSocketClosed(object sender, EventArgs e)
        {
            var evnt = Disconnected;
            if (evnt != null)
            {
                evnt();
            }

            Reconnect();
        }

        private void OnSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            var evnt = Error;
            if (evnt != null)
            {
                evnt(e.Exception);
            }

            Reconnect();
        }

        private void OnSocketMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var evnt = MessageReceived;
            if (evnt != null)
            {
                var strMessage = e.Message;
                var message = JsonConvert.DeserializeObject<Message>(strMessage);
                evnt(message.Topic, message);
            }
        }

        private void OnSocketOpened(object sender, EventArgs e)
        {
            var evnt = Connected;
            if (evnt != null)
            {
                var invocationList = evnt.GetInvocationList();
                foreach (var ev in invocationList)
                {
                    var evAction = ev as Action;
                    if (evAction != null)
                    {
                        evAction.BeginInvoke(evAction.EndInvoke, null);
                    }
                }
            }
        }

        private MessageBrokerWatcher CreateBrokerWatcher()
        {
            var configuration = new MessageBrokerConfiguration(_logger);

            _brokerPort = configuration.GetBrokerPort(_brokerPort);

            var brokerExePath = configuration.GetBrokerExePath();
            _logger.Info(string.Format("Resolved broker exe path: {0}", brokerExePath));

            var brokerLibPath = configuration.GetBrokerLibPath();
            _logger.Info(string.Format("Resolved broker lib path: {0}", brokerLibPath));

            var brokerLoggingConfig = configuration.GetBrokerLoggingConfiguration();
            _logger.Info(string.Format("Resolved broker logging path: {0}", brokerLoggingConfig));

            return new MessageBrokerWatcher(_logger, "-jar ", brokerLibPath, brokerExePath, brokerLoggingConfig, _brokerPort);
        }
    }
}