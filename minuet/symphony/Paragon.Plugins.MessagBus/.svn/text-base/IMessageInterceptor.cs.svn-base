namespace Paragon.Plugins.MessageBus
{
    public interface IMessageInterceptor
    {
        bool OnGetState(string topic);
        bool OnMessageReceived(string topic, object message);
        bool OnPublishMessage(string topic, object msg);
        bool OnSendMessage(string topic, object msg);
        bool OnSubscribe(string topic);
        bool OnUnsubscribe(string topic);
    }
}