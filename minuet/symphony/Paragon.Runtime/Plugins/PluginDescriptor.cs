using System.Collections.Generic;

namespace Paragon.Runtime.Plugins
{
    public enum PluginProcess
    {
        Browser,
        Renderer
    }

    /// <summary>
    /// Defines the metadata required to interact with a plugin, locally or remotely.
    /// </summary>
    public class PluginDescriptor
    {
        /// <summary>
        /// For a static plugin this is the full JavaScript path for the object. For a dynamic plugin it is a Guid.
        /// </summary>
        public string PluginId { get; set; }

        public List<MethodDescriptor> Methods { get; set; }

        public List<string> Events { get; set; }
    }

    public class MethodDescriptor
    {
        public string MethodName { get; set; }
        public bool HasCallbackParameter { get; set; }
    }
}