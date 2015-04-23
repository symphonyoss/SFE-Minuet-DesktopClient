namespace Paragon.Plugins
{
    public interface IMessageBusPlugin
    {
        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnConnected;

        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnDisconnected;

        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnError;

        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnMessage;

        [JavaScriptPluginMember]
        void PublishMessage(string topic, object message);

        [JavaScriptPluginMember]
        void SendMessage(string topic, object message, string rid);

        [JavaScriptPluginMember]
        void Subscribe(string topic, string rid);

        [JavaScriptPluginMember]
        void Unsubscribe(string topic, string rid);
    }
}