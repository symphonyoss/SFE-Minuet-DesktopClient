using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins.TypeConversion
{
    public interface INativeTypeMemberConverter
    {
        string MemberName { get; }

        void SetNativeValue(object nativeObject, CefV8Value cefObject);

        void SetCefValue(CefV8Value cefObject, object nativeObject);
    }
}