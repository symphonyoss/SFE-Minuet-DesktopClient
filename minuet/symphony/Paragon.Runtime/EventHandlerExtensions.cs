using System;
using System.Threading;
using Paragon.Plugins;

namespace Paragon.Runtime
{
    public static class EventHandlerExtensions
    {
        public static void Raise<T>(this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            EventHandler<T> local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local(sender, args);
            }
        }

        public static void Raise(this EventHandler handler, object sender, EventArgs args)
        {
            EventHandler local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local(sender, args);
            }
        }

        public static void Raise(this JavaScriptPluginCallback handler)
        {
            JavaScriptPluginCallback local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local();
            }
        }

        public static void Raise(this JavaScriptPluginCallback handler, Func<object[]> getArgs)
        {
            JavaScriptPluginCallback local = Interlocked.CompareExchange(ref handler, null, null);
            if (local != null)
            {
                local(getArgs());
            }
        }
    }
}