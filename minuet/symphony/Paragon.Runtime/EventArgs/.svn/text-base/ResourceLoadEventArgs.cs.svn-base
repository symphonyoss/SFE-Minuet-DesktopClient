using System.ComponentModel;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class ResourceLoadEventArgs : CancelEventArgs
    {
        public ResourceLoadEventArgs(string url, CefResourceType resourceType)
        {
            Url = url;
            ResourceType = resourceType;
        }

        public string Url { get; private set; }
        public CefResourceType ResourceType { get; private set; }
    }
}