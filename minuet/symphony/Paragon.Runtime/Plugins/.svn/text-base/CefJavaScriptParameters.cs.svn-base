using System.Reflection;
using Paragon.Runtime.Plugins.TypeConversion;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines translation between parameters received from V8 and the C# parameters for the plugin method being invoked.
    /// </summary>
    public class CefJavaScriptParameters : IJavaScriptParameters
    {
        private readonly CefV8Value[] _parameters;

        public CefJavaScriptParameters(CefV8Value[] parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Convert the parameter values from the caller into native C# objects that match the supplied parameter definitions.
        /// </summary>
        /// <param name="parameterInfos">The parameter definitions for the method to be invoked.</param>
        /// <param name="pluginManager">Plugin Manager instance</param>
        /// <returns>Native C# objects matching <param name="parameterInfos"></param></returns>
        /// <remarks>
        /// The method will be called by <see cref="JavaScriptPlugin.InvokeFunction"/> on the calling thread, *before* dispatching to a background thread for execution.
        /// </remarks>
        public object[] GetConvertedParameters(ParameterInfo[] parameterInfos, IPluginManager pluginManager)
        {
            // TODO: handle params method definition
            // TODO: ensure sufficient values have been provided (factoring in parameters with default values)

            var arguments = new object[parameterInfos.Length];
            for (var parameterInfoIndex = 0; parameterInfoIndex < parameterInfos.Length; ++parameterInfoIndex)
            {
                if (parameterInfoIndex < _parameters.Length)
                {
                    var parameterToken = _parameters[parameterInfoIndex];
                    arguments[parameterInfoIndex] = CefNativeValueConverter.ToNative(parameterToken, parameterInfos[parameterInfoIndex].ParameterType);
                }
                else
                {
                    arguments[parameterInfoIndex] = null;
                }
            }
            return arguments;
        }
    }
}