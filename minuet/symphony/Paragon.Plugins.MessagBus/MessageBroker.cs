using System.Net;
using Newtonsoft.Json;
using System;
using System.Threading;
using WebSocket4Net;

namespace Paragon.Plugins.MessageBus
{
    public class MessageBroker : IMessageBroker
    {
        public static int DefaultPort = 65534;
        private static int _idcount = 1;
        private readonly TimeSpan _reconnectFrequency = TimeSpan.FromSeconds(10);
        private long _disconnectRequested;
        private ILogger _logger;
        private WebSocket _socket;
        private MessageBrokerWatcher _watcher;
        public event Action Connected;
        public event Action Disconnected;
        public event Action<Exception> Error;
        public event Action<string, object> MessageReceived;
        private int _brokerPort = DefaultPort;
        private string _domain;

        public MessageBroker(ILogger logger)
            : this(logger, null)
        {
        }

        public MessageBroker(ILogger logger, MessageBrokerWatcher watcher)
            : this(logger, watcher, DefaultPort)
        {
        }

        public MessageBroker(ILogger logger, MessageBrokerWatcher watcher, int brokerPort)
        {
            _logger = logger;
            _watcher = watcher;
            if (brokerPort != DefaultPort)
                _brokerPort = brokerPort;
        }


        public string Domain
        {
            get
            {
                if (string.IsNullOrEmpty(_domain))
                {
                    var hostnameParts = Dns.GetHostEntry("localhost").HostName.Split('.');
                    _domain = (hostnameParts.Length > 1) ? (hostnameParts[1]) : (string.Empty);
                }
                return _domain;
            }
        }

        public void Connect()
        {
            if (_watcher == null)
            {
                _watcher = CreateBrokerWatcher();
            }
            if( _watcher != null )
                _watcher.Start(new Action(OnBrokerProcessStarted));
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

        public void Reconnect()
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
            if (_socket != null)
            {
                _socket.Opened -= OnSocketOpened;
                _socket.Error -= OnSocketError;
                _socket.Closed -= OnSocketClosed;
                _socket.MessageReceived -= OnSocketMessageReceived;
                if (_socket.State == WebSocketState.Open ||
                   _socket.State == WebSocketState.Connecting)
                {
                    _socket.Close();
                }
                _socket = null;
            }
        }

        private void OnSocketClosed(object sender, EventArgs e)
        {
            if (Disconnected != null)
            {
                Disconnected();
                Reconnect();
            }
        }

        private void OnSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (Error != null)
            {
                Error(e.Exception);
                Reconnect();
            }
        }

        private void OnSocketMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var strMessage = e.Message;
            var message = JsonConvert.DeserializeObject<Message>(strMessage);

            if (MessageReceived != null)
            {
                MessageReceived(message.Topic, message);
            }
        }

        private void OnSocketOpened(object sender, EventArgs e)
        {
            if (Connected != null)
            {
                Connected();
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