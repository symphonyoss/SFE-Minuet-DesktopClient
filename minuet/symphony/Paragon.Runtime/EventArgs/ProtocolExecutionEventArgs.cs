using System;

namespace Paragon.Runtime
{
    public class ProtocolExecutionEventArgs : EventArgs
    {
        public ProtocolExecutionEventArgs(string url)
        {
            Url = url;
            Allow = false;
        }

        public string Url { get; private set; }
        public bool Allow { get; set; }
    }
}
