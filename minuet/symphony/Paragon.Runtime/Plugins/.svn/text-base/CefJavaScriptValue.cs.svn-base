using System.Reflection;
using Paragon.Runtime.Plugins.TypeConversion;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public class CefJavaScriptValue : IJavaScriptValue
    {
        private readonly CefV8Value _cefValue;

        public CefJavaScriptValue(CefV8Value cefValue)
        {
            _cefValue = cefValue;
        }

        public object GetConvertedValue(PropertyInfo propertyInfo)
        {
            return CefNativeValueConverter.ToNative(_cefValue, propertyInfo.PropertyType);
        }
    }
}