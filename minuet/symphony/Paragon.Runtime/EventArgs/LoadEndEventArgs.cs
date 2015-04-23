using System.ComponentModel;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class LoadEndEventArgs : CancelEventArgs
    {
        public LoadEndEventArgs(CefFrame frame)
        {
            Frame = frame;
            Url = frame.Url;
        }

        public CefFrame Frame { get; private set; }
        public string Url { get; private set; }
    }
}