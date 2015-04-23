using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public interface IRenderSideMessageRouter : IMessageRouter
    {
        event Action<IPluginContext> OnPluginContextCreated;
        void BrowserDestroyed(CefBrowser browser);
        void ContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context);
        void ContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context);
        void InitializePlugins(CefListValue browserPlugins);
        void LocalCallbackInvoked(LocalRenderCallInfo call, object data, int errorCode, string error);
    }
}