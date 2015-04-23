using System.Reflection;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins.TypeConversion
{
    public class NativeTypePropertyConverter : INativeTypeMemberConverter
    {
        private readonly PropertyInfo _propertyInfo;

        public NativeTypePropertyConverter(string scriptName, PropertyInfo propertyInfo)
        {
            MemberName = scriptName;
            _propertyInfo = propertyInfo;
        }

        public string MemberName { get; private set; }

        public void SetNativeValue(object nativeObject, CefV8Value cefObject)
        {
            var propertyValue = CefNativeValueConverter.ToNative(
                cefObject.GetValue(MemberName),
                _propertyInfo.PropertyType);
            _propertyInfo.SetValue(nativeObject, propertyValue, null);
        }

        public void SetCefValue(CefV8Value cefObject, object nativeObject)
        {
            var propertyValue = _propertyInfo.GetValue(nativeObject, null);
            var cefValue = propertyValue != null
                ? CefNativeValueConverter.ToCef(propertyValue, _propertyInfo.PropertyType)
                : CefV8Value.CreateNull();
            cefObject.SetValue(MemberName, cefValue, CefV8PropertyAttribute.None);
        }
    }
}