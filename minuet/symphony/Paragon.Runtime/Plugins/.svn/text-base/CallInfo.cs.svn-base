using System;

namespace Paragon.Runtime.Plugins
{
    public abstract class CallInfo : IDisposable
    {
        public const int ErrorCodeCallCanceled = -1;
        public const string ErrorCallCanceled = "The call has been canceled";
        public const string Call = "GS_Call";
        private bool _disposed;

        protected CallInfo(PluginMessage requestMessage, string pluginId = null)
        {
            RequestMessage = requestMessage;
            PluginId = pluginId;
        }

        public PluginMessage RequestMessage { get; private set; }

        public string PluginId { get; private set; }

        public int ContextId
        {
            get { return RequestMessage.ContextId; }
        }

        public bool IsEventListener
        {
            get { return RequestMessage.MessageType == PluginMessageType.AddListener; }
        }

        public bool IsRetained
        {
            get { return (RequestMessage.MessageType & PluginMessageType.Retained) == PluginMessageType.Retained; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}