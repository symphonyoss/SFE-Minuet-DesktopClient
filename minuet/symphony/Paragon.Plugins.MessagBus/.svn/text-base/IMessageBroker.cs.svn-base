using System;

namespace Paragon.Plugins.MessageBus
{
    public interface IMessageBroker
    {
        event Action Connected;
        event Action Disconnected;
        event Action<Exception> Error;
        event Action<string, object> MessageReceived;

        void Connect();
        void Disconnect();
        string GetState(string topic);
        void PublishMessage(string topic, object msg);
        void SendMessage(string address, object msg, string responseId);
        void Subscribe(string topic, string responseId);
        void Unsubscribe(string topic, string responseId);
    }
}