using System;

namespace Paragon.Plugins
{
    /// <summary>
    /// Marks an event implemented by a dynamic plugin type as indicating the closing of an instance of that dynamic plugin.
    /// This event is used by the runtime to release the cached reference to the plugin object.
    /// This event should be of the JavaScriptPluginEventHandler delegate type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event)]
    public class JavaScriptDisposeAttribute : Attribute
    {
    }
}