using System;
using System.Linq;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public class PluginMethodV8Handler : CefV8Handler
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly IV8Plugin _plugin;

        public PluginMethodV8Handler(IV8Plugin plugin)
        {
            _plugin = plugin;
        }

        /// <summary>
        /// Handle execution of the function identified by |name|. |object| is the
        /// receiver ('this' object) of the function. |arguments| is the list of
        /// arguments passed to the function. If execution succeeds set |retval| to the
        /// function return value. If execution fails set |exception| to the exception
        /// that will be thrown. Return true if execution was handled.
        /// </summary>
        /// <param name="name">The name of the function to execute.</param>
        /// <param name="obj">The receiver ('this' object) of the function.</param>
        /// <param name="arguments">The list of arguments passed to the function</param>
        /// <param name="returnValue">If execution succeeds set |retval| to the function return value.</param>
        /// <param name="exception">If execution fails set |exception| to the exception that will be thrown.</param>
        /// <returns>Return true if execution was handled.</returns>
        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            exception = null;
            returnValue = null;

            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var methodDescriptor = _plugin.Descriptor.Methods.Find(descriptor => descriptor.MethodName == name);
                    if (methodDescriptor == null)
                    {
                        Logger.Error("Plugin method {0} {1} not found", _plugin.Descriptor.PluginId, name);
                        return false;
                    }

                    var context = CefV8Context.GetCurrentContext();

                    var v8Callback = arguments != null && arguments.Length != 0 && arguments[arguments.Length - 1].IsFunction
                        ? arguments[arguments.Length - 1]
                        : null;
                    var parameters = v8Callback != null ? arguments.Take(arguments.Length - 1).ToArray() : arguments;

                    _plugin.ExecuteFunction(context, name, parameters, v8Callback, out returnValue, out exception);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Execution of plugin method {0} {1} failed: {2}", _plugin.Descriptor.PluginId, name, ex);
                exception = ex.Message;
            }

            returnValue = CefV8Value.CreateUndefined();

            return false;
        }
    }
}