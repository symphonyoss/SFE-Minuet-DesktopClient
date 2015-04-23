using System;

namespace Paragon.Plugins
{
    public class ProtocolInvocationEventArgs : EventArgs
    {
        public ProtocolInvocationEventArgs(string uri)
        {
            Uri = uri;
        }

        public string Uri { get; private set; }
    }
}