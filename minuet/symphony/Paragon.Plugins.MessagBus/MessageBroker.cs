using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using WebSocket4Net;

namespace Paragon.Plugins.MessageBus
{
    public class MessageBroker : IMessageBroker
    {
        public static int DefaultPort = 65534;
        private const string DefaultBrokerExePath = "\\\\firmwide.corp.gs.com\\appstore\\51\\Production\\14974\\bin\\java.exe";
        private const string DefaultBrokerLibPath = "\\\\firmwide\\appstore\\1188\\Production\\messagebus.jar";
        private const string DefaultBrokerLibPath2 = "\\\\firmwide\\appstore\\1188\\Test\\messagebus.jar";
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
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var brokerLibPath = GetSetting(appConfig,"BrokerLibraryPath");
            var brokerExePath = GetSetting(appConfig, "BrokerExePath");
            var brokerLoggingConfig = GetSetting(appConfig, "BrokerLoggingConfigFile");
            var brokerPort = GetSetting(appConfig, "BrokerPort");

            if (!string.IsNullOrEmpty(brokerPort))
            {
                int portNum;
                if (int.TryParse(brokerPort, out portNum))
                {
                    _brokerPort = portNum;
                }
            }

            if(string.IsNullOrEmpty(brokerExePath) || brokerExePath.Equals("detect", StringComparison.InvariantCultureIgnoreCase) )
            {
                brokerExePath = DetectJavaPath();
            }

            if (string.IsNullOrEmpty(brokerLibPath) || brokerLibPath.Equals("detect", StringComparison.InvariantCultureIgnoreCase))
            {
                brokerLibPath = DetectBrokerLibPath();
            }

            if (string.IsNullOrEmpty(brokerLoggingConfig) || brokerLoggingConfig.Equals("detect", StringComparison.InvariantCultureIgnoreCase))
            {
                var file = Path.GetDirectoryName(GetType().Assembly.Location) + "\\messagebroker.log4j.xml";
                if (File.Exists(file))
                    brokerLoggingConfig = file;
                else
                    brokerLoggingConfig = string.Empty;
            }

            return new MessageBrokerWatcher(_logger, "-jar ", brokerLibPath, brokerExePath, brokerLoggingConfig, _brokerPort);
        }

        private string GetSetting(Configuration config, string key)
        {
            try
            {
                var entry = config.AppSettings.Settings[key];
                return entry == null ? null : entry.Value;
            }
            catch
            {
                return null;
            }
        }

        public static string DetectJavaPath()
        {
            return DefaultBrokerExePath;
        }

        public static string DetectBrokerLibPath()
        {
            if (File.Exists(DefaultBrokerLibPath))
                return DefaultBrokerLibPath;
            if (File.Exists(DefaultBrokerLibPath2))
                return DefaultBrokerLibPath2;
            return string.Empty;
        }
    }
}