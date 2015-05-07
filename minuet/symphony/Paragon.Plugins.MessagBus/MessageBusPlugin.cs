using System;
using System.Collections.Generic;
using System.Threading;
using Paragon.Plugins.MessageBus.Annotations;
using WebSocket4Net;
    
namespace Paragon.Plugins.MessageBus
{
    [JavaScriptPlugin(Name = "paragon.messagebus")]
    public class MessageBusPlugin : ParagonPlugin, IMessageBusPlugin
    {
        private IMessageBroker _broker;
        private long _connectedToBroker;
        private IMessageInterceptor _interceptor;
        private ILogger _logger;
        private List<string> _subscriptions = new List<string>();
        
        public MessageBusPlugin()
            : this(null, null)
        {
        }

        public MessageBusPlugin(ILogger logger)
            : this(logger, null)
        {
        }

        public MessageBusPlugin(ILogger logger, IMessageBroker broker)
        {
            _logger = logger;
            _broker = broker;
        }

        public string MessageBusId { get; set; }

        [JavaScriptPluginMember]
        public event JavaScriptPluginCallback OnConnected;

        [JavaScriptPluginMember]
        public event JavaScriptPluginCallback OnDisconnected;

        [JavaScriptPluginMember]
        public event JavaScriptPluginCallback OnError;

        [JavaScriptPluginMember]
        public event JavaScriptPluginCallback OnMessage;

        [JavaScriptPluginMember]
        public void PublishMessage(string topic, object message)
        {
            if (_interceptor == null || !_interceptor.OnPublishMessage(topic, message))
            {
                if (_broker != null)
                {
                    _broker.PublishMessage(topic, message);
                }
            }
        }

        [JavaScriptPluginMember]
        public void SendMessage(string topic, object message, string rid)
        {
            if (_interceptor == null || !_interceptor.OnSendMessage(topic, message))
            {
                if (_broker != null)
                {
                    _broker.SendMessage(topic, message, rid);
                }
            }
        }

        [JavaScriptPluginMember]
        public void Subscribe(string topic, string rid)
        {
            if (_interceptor == null || !_interceptor.OnSubscribe(topic))
            {
                if(!_subscriptions.Contains(topic))
                    _subscriptions.Add(topic);
                if (_broker != null)
                {
                    _broker.Subscribe(topic, rid);
                }
            }
        }

        [JavaScriptPluginMember]
        public void Unsubscribe(string topic, string rid)
        {
            if (_interceptor == null || !_interceptor.OnUnsubscribe(topic))
            {
                if (_subscriptions.Contains(topic))
                    _subscriptions.Remove(topic);

                if (_broker != null)
                {
                    _broker.Unsubscribe(topic, rid);
                }
            }
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public string GetState(string topic)
        {
            var state = WebSocketState.None.ToString();
            if (_interceptor == null || !_interceptor.OnGetState(topic))
            {
                if (_broker != null)
                {
                    state = _broker.GetState(topic);
                }
            }
            return state;
        }

        public void RecieveMessage(string topic, object message)
        {
            if (OnMessage != null)
            {
                OnMessage(topic, message);
            }
        }

        public void RegisterMessageInterceptor(IMessageInterceptor interceptor)
        {
            _interceptor = interceptor;
        }

        protected override void OnInitialize()
        {
            if (string.IsNullOrEmpty(MessageBusId) && Application != null)
            {
                _logger = Application.Logger;
                MessageBusId = Application.Metadata.InstanceId;
            }

            _logger.Debug("MessageBusPlugin Initialize");

            try
            {
                if (_broker == null)
                {
                    _broker = new MessageBroker(_logger);
                }

                if (_broker != null)
                {
                    _broker.Connected += OnConnectedToBroker;
                    _broker.Error += OnBrokerError;
                    _broker.MessageReceived += OnBrokerMessageRecieved;
                }

                Connect();
            }
            catch (Exception ex)
            {
                _logger.Error("MessageBusPlugin Initialization failed.", ex);
            }
        }

        protected override void OnShutdown()
        {
            _logger.Debug("MessageBusPlugin shutdown");

            Disconnect();

            if (_broker != null)
            {
                _broker.Connected -= OnConnectedToBroker;
                _broker.Error -= OnBrokerError;
                _broker.MessageReceived -= OnBrokerMessageRecieved;
                _broker = null;
            }
            base.OnShutdown();
        }

        private void Connect()
        {
            if (_broker != null)
            {
                _broker.Connect();
            }
        }

        private void Disconnect()
        {
            if (_broker != null)
            {
                _broker.Disconnect();
            }
        }

        private void OnBrokerError(Exception ex)
        {
            _logger.Error("Error in broker connection :", ex);
            if (OnError != null)
            {
                OnError(ex.Message);
            }
        }

        private void OnBrokerMessageRecieved(string topic, object message)
        {
            if (_interceptor == null || !_interceptor.OnMessageReceived(topic, message))
            {
                RecieveMessage(topic, message);
            }
        }

        private void OnConnectedToBroker()
        {
            _logger.Info("Connected to broker.");

            Interlocked.Exchange(ref _connectedToBroker, 1);

            _broker.Disconnected += OnDisconnectedFromBroker;
            _broker.Subscribe(MessageBusId, null);
            foreach (string topic in _subscriptions)
                _broker.Subscribe(topic, null);

            var evnt = OnConnected;
            if (evnt != null)
            {
                evnt();
            }
        }

        private void OnDisconnectedFromBroker()
        {
            Interlocked.Exchange(ref _connectedToBroker, 0);
            _broker.Disconnected -= OnDisconnectedFromBroker;
            _logger.Warn("Disconnected from the broker.");

            var evnt = OnDisconnected;
            if (evnt != null)
            {
                evnt();
            }
        }
    }
}