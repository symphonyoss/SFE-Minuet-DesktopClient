using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public class PluginV8Accessor : CefV8Accessor
    {
        protected override bool Get(string name, CefV8Value obj, out CefV8Value returnValue, out string exception)
        {
            var objectUserData = obj.GetUserData() as V8PluginAdapter;
            var scriptObject = objectUserData != null ? objectUserData.Plugin : null;
            if (scriptObject != null)
            {
                return scriptObject.GetProperty(name, out returnValue, out exception);
            }

            exception = null;
            returnValue = CefV8Value.CreateUndefined();
            return false;
        }

        protected override bool Set(string name, CefV8Value obj, CefV8Value value, out string exception)
        {
            var objectUserData = obj.GetUserData() as V8PluginAdapter;
            var scriptObject = objectUserData != null ? objectUserData.Plugin : null;
            if (scriptObject != null)
            {
                return scriptObject.SetProperty(name, value, out exception);
            }

            exception = null;
            return false;
        }
    }
}