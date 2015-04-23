using System;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Decouples the ability to call back into V8 in response to a function call or event raised.
    /// 
    /// The implementation of this interface handles the type conversion to the appropriate V8 value hierarchy.
    /// </summary>
    public interface IV8Callback : IDisposable
    {
        Guid Identifier { get; }

        void Invoke(IV8PluginRouter router, CefV8Context context, object result, int errorCode, string error);
    }
}