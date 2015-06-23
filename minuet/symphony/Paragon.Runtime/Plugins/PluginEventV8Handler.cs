using System;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public class PluginEventV8Handler : CefV8Handler
    {
        public const string MethodNameAddListener = "addListener";
        public const string MethodNameRemoveListener = "removeListener";
        public const string MethodNameHasListener = "hasListener";
        public const string MethodNameHasListeners = "hasListeners";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly string _eventName;
        private readonly IV8Plugin _plugin;

        public PluginEventV8Handler(IV8Plugin pluginObject, string eventName)
        {
            _plugin = pluginObject;
            _eventName = eventName;
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
        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue,
            out string exception)
        {
            if (string.IsNullOrEmpty(name))
            {
                returnValue = CefV8Value.CreateUndefined();
                exception = "Unknown event name";
                return false;
            }

            exception = null;
            returnValue = null;

            try
            {
                switch (name)
                {
                    case MethodNameAddListener:
                    case MethodNameRemoveListener:
                    {
                        if (arguments == null || arguments.Length != 1 || !arguments[0].IsFunction)
                        {
                            exception = "Expected a callback as the only argument";
                        }
                        else
                        {
                            var context = CefV8Context.GetCurrentContext(); // TODO should this be disposed?
                            var v8Callback = arguments[0];

                            switch (name)
                            {
                                case MethodNameAddListener:
                                    _plugin.AddEventListener(context, _eventName, v8Callback);
                                    break;
                                case MethodNameRemoveListener:
                                    _plugin.RemoveEventListener(context, _eventName, v8Callback);
                                    break;
                            }
                            return true;
                        }
                        break;
                    }
                    case MethodNameHasListener:
                    {
                        if (arguments == null || arguments.Length != 1 || !arguments[0].IsFunction)
                        {
                            exception = "Expected a callback as the only argument";
                        }
                        else
                        {
                            var v8Callback = arguments[0];
                            var hasListener = _plugin.HasEventListener(_eventName, v8Callback);
                            returnValue = CefV8Value.CreateBool(hasListener);
                            return true;
                        }
                        break;
                    }
                    case MethodNameHasListeners:
                    {
                        var hasListener = _plugin.HasEventListeners(_eventName);
                        returnValue = CefV8Value.CreateBool(hasListener);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Execution of plugin event method {0} {1} {2} failed: {3}",
                    _plugin.Descriptor.PluginId,
                    _eventName,
                    name,
                    ex);

                exception = ex.Message;
            }

            returnValue = CefV8Value.CreateUndefined();
            return false;
        }
    }
}