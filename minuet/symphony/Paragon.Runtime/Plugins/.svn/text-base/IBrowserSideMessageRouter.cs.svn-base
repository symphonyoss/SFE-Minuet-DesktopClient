using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public interface IBrowserSideMessageRouter : IMessageRouter, IBrowserCallResponseHandler
    {
        IPluginManager PluginManager { get; }
        CefListValue CreatePluginInitMessage();
    }
}