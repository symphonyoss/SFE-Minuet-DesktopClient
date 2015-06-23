using System.ComponentModel;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class ResourceLoadEventArgs : CancelEventArgs
    {
        public ResourceLoadEventArgs(string url, CefResourceType resourceType, CefRequestCallback callback)
        {
            Url = url;
            ResourceType = resourceType;
            Callback = callback;
        }

        public string Url { get; private set; }
        public CefResourceType ResourceType { get; private set; }
        public CefRequestCallback Callback { get; private set; }
    }
}