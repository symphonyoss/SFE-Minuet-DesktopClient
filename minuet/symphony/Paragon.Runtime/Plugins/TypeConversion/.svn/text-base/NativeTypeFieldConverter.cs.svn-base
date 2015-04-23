using System.Reflection;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins.TypeConversion
{
    public class NativeTypeFieldConverter : INativeTypeMemberConverter
    {
        private readonly FieldInfo _fieldInfo;

        public NativeTypeFieldConverter(string scriptName, FieldInfo fieldInfo)
        {
            MemberName = scriptName;
            _fieldInfo = fieldInfo;
        }

        public string MemberName { get; private set; }

        public void SetNativeValue(object nativeObject, CefV8Value cefObject)
        {
            var fieldValue = CefNativeValueConverter.ToNative(
                cefObject.GetValue(MemberName),
                _fieldInfo.FieldType);
            _fieldInfo.SetValue(nativeObject, fieldValue);
        }

        public void SetCefValue(CefV8Value cefObject, object nativeObject)
        {
            var fieldValue = _fieldInfo.GetValue(nativeObject);
            var cefValue = fieldValue != null
                ? CefNativeValueConverter.ToCef(fieldValue, _fieldInfo.FieldType)
                : CefV8Value.CreateNull();
            cefObject.SetValue(MemberName, cefValue, CefV8PropertyAttribute.None);
        }
    }
}