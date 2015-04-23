using Paragon.Plugins;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines a callback which can be wrapped in <see cref="JavaScriptPluginCallback"/> and passed to a native plugin object method.
    /// </summary>
    public interface IJavaScriptParameterCallback
    {
        /// <summary>
        /// Invoked by a native plugin object which sees this callback as a <see cref="JavaScriptPluginCallback"/>.
        /// </summary>
        /// <param name="data">The result of the method (assuming it didn't throw an exception), or the payload of the event.</param>
        void Invoke(params object[] data);
    }
}