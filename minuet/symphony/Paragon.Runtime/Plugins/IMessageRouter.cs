using System;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public interface IMessageRouter : IDisposable
    {
        bool ProcessCefMessage(CefBrowser browser, CefProcessMessage message);
    }
}