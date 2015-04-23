using System.Reflection;

namespace Paragon.Runtime.Plugins
{
    public interface IJavaScriptValue
    {
        object GetConvertedValue(PropertyInfo propertyInfo);
    }
}