using System;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines the types of <see cref="PluginMessage"/> that can be sent and received.
    /// </summary>
    [Flags]
    public enum PluginMessageType
    {
        Invalid = 0,

        /// <summary>
        /// Configure plugins for a browser instance.
        /// </summary>
        InitializePlugins = 1,

        /// <summary>
        /// The invocation of a function exposed by a plugin.
        /// </summary>
        FunctionInvoke = 2,

        /// <summary>
        /// The response to a function invocation.
        /// </summary>
        FunctionCallback = 4,

        /// <summary>
        /// A callback passed into a function invocation rather than 
        /// being invoked by the framework when the function completes.
        /// </summary>
        ParameterCallback = FunctionCallback | Retained,

        /// <summary>
        /// The addition of a remote callback to a plugin event.
        /// </summary>
        AddListener = 32 | Retained,

        /// <summary>
        /// Sent to registered listener callbacks when an event is fired.
        /// </summary>
        EventFired = 64,

        /// <summary>
        /// Indicates the message is a callback or listener that needs to be 
        /// retained until it is explicitly unsubscribed or the context is released.
        /// </summary>
        Retained = 128,

        /// <summary>
        /// Remove a retained callback or listener
        /// </summary>
        RemoveRetained = 256,
    }
}