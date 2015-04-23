using System;

namespace Paragon.Plugins
{
    /// <summary>
    /// Presence of this attribute on a class indicates that it is a plugin. 
    /// Primarily, there are two types of plugins: those that are pre-populated into the JS context 
    /// and those that are returned dynamically by other plugins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JavaScriptPluginAttribute : Attribute
    {
        public JavaScriptPluginAttribute()
        {
            IsBrowserSide = false;
        }

        /// <summary>
        /// Name of the plugin. If this is not set, the plugin will be considered dynamic and will not be pre-populated into a JavaScript context.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates that the plugin should run on the browser process side.
        /// </summary>
        public bool IsBrowserSide { get; set; }

        /// <summary>
        /// Gets or sets an enum value describing the thread that should be used when invoking methods on the plugin.
        /// </summary>
        public CallbackThread CallbackThread { get; set; }
    }
}