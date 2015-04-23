using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins.TypeConversion
{
    public static class CefNativeValueConverter
    {
        private static readonly Dictionary<Type, CefNativeTypeConverter> ObjectTypeConverters = new Dictionary<Type, CefNativeTypeConverter>();

        public static object ToNative(CefV8Value obj, Type targetType)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            object retVal = null;

            if ((targetType == typeof (JObject) && obj.IsObject) ||
                (targetType == typeof (JArray) && obj.IsArray) ||
                targetType == typeof (JValue) ||
                targetType == typeof (JToken))
            {
                retVal = CefJsonValueConverter.ToJToken(obj);
            }
            else if (!obj.IsNull && !obj.IsUndefined)
            {
                if (obj.IsBool)
                {
                    retVal = obj.GetBoolValue();
                }
                else if (obj.IsDouble || obj.IsInt || obj.IsUInt)
                {
                    // CEF doesn't have a long data type so they are treated as doubles
                    // To avoid overflow scenarios, IsDouble must be checked before IsInt because flags will be true
                    retVal = obj.GetDoubleValue();
                }
                else if (obj.IsString)
                {
                    retVal = obj.GetStringValue();
                }
                else if (obj.IsDate)
                {
                    retVal = obj.GetDateValue();
                }
                else if (obj.IsArray)
                {
                    if (targetType.IsArray)
                    {
                        if (targetType.GetArrayRank() != 1)
                        {
                            throw new Exception("Cannot handle multidimensional arrays");
                        }

                        var v8ArrayLength = obj.GetArrayLength();
                        var elementType = targetType.GetElementType();
                        var array = Array.CreateInstance(elementType, v8ArrayLength);
                        for (var v8ArrayIndex = 0; v8ArrayIndex < v8ArrayLength; ++v8ArrayIndex)
                        {
                            var elementToken = ToNative(obj.GetValue(v8ArrayIndex), elementType);
                            array.SetValue(elementToken, v8ArrayIndex);
                        }

                        retVal = array;
                    }
                }
                else if (obj.IsObject)
                {
                    var pluginAdapter = V8PluginAdapter.FromCefObject(obj);
                    if (pluginAdapter != null)
                    {
                        throw new Exception("Passing plugins as parameters not yet supported");
                    }

                    var converter = GetTypeConverter(targetType);
                    if (converter != null)
                    {
                        retVal = converter.ToNative(obj);
                    }
                }
                else if (obj.IsFunction)
                {
                    // TODO : Throw an exception?
                }

                if (retVal != null && !targetType.IsInstanceOfType(retVal))
                {
                    retVal = Convert.ChangeType(retVal, targetType);
                }
            }

            return retVal;
        }

        public static CefV8Value ToCef(object nativeObject)
        {
            return ToCef(nativeObject, null);
        }

        public static CefV8Value ToCef(object nativeObject, Type type)
        {
            if (nativeObject == null)
            {
                if (type != null && type == typeof (void))
                {
                    return CefV8Value.CreateUndefined();
                }
                return CefV8Value.CreateNull();
            }

            if (nativeObject is bool)
            {
                return CefV8Value.CreateBool((bool) nativeObject);
            }

            var stringValue = nativeObject as string;
            if (stringValue != null)
            {
                return CefV8Value.CreateString(stringValue);
            }

            if (nativeObject is int || nativeObject is byte ||
                nativeObject is ushort || nativeObject is sbyte ||
                nativeObject is short || nativeObject is char)
            {
                return CefV8Value.CreateInt(Convert.ToInt32(nativeObject));
            }

            if (nativeObject is double || nativeObject is decimal ||
                nativeObject is long || nativeObject is uint ||
                nativeObject is ulong || nativeObject is float)
            {
                return CefV8Value.CreateDouble(Convert.ToDouble(nativeObject));
            }

            var nativeArray = nativeObject as Array;
            if (nativeArray != null)
            {
                var cefArray = CefV8Value.CreateArray(nativeArray.Length);
                for (var i = 0; i < nativeArray.Length; i++)
                {
                    var nativeArrayItem = nativeArray.GetValue(i);
                    var cefArrayItem = ToCef(nativeArrayItem, null);
                    cefArray.SetValue(i, cefArrayItem);
                }
                return cefArray;
            }

            var jsonToken = nativeObject as JToken;
            if (jsonToken != null)
            {
                return CefJsonValueConverter.ToCef(jsonToken);
            }

            if (type == null)
            {
                type = nativeObject.GetType();
            }

            var typeConverter = GetTypeConverter(type);
            if (typeConverter != null)
            {
                return typeConverter.ToCef(nativeObject);
            }

            throw new Exception(string.Format("Cannot convert '{0}' object from CLR to CEF.", type.FullName));
        }

        private static CefNativeTypeConverter GetTypeConverter(Type nativeType)
        {
            CefNativeTypeConverter converter;
            if (ObjectTypeConverters.TryGetValue(nativeType, out converter))
            {
                return converter;
            }
            if (nativeType.IsClass || (nativeType.IsValueType && !nativeType.IsPrimitive && !nativeType.IsEnum))
            {
                converter = new CefNativeTypeConverter(nativeType);
                ObjectTypeConverters[nativeType] = converter;
            }
            return converter;
        }
    }
}