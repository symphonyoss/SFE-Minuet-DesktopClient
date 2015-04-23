using System.Reflection;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines translation between parameters received from V8 or remotely, and the C# parameters for the plugin method being invoked.
    /// </summary>
    public interface IJavaScriptParameters
    {
        /// <summary>
        /// Convert the parameter values from the caller into native C# objects that match the supplied parameter definitions.
        /// </summary>
        /// <param name="parameterInfos">
        /// The parameter definitions for the method to be invoked.
        /// </param>
        /// <param name="pluginManager">
        /// The plugin manager used to lookup dynamic plugins passed in as function arguments
        /// </param>
        /// <returns>
        /// Native C# objects matching <param name="parameterInfos"></param>
        /// </returns>
        /// <remarks>
        /// The method will be called by <see cref="JavaScriptPlugin"/> on the calling thread, *before* dispatching to a background thread for execution.
        /// </remarks>
        object[] GetConvertedParameters(ParameterInfo[] parameterInfos, IPluginManager pluginManager);
    }
}