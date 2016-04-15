//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System;
using Newtonsoft.Json.Linq;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins.TypeConversion
{
    public static class CefJsonValueConverter
    {
        public static CefV8Value ToCef(JToken jsonToken)
        {
            switch (jsonToken.Type)
            {
                case JTokenType.Object:
                {
                    var jObject = (JObject) jsonToken;
                    var plainObject = CefV8Value.CreateObject(null);
                    foreach (var jsonProperty in jObject.Properties())
                    {
                        plainObject.SetValue(jsonProperty.Name, ToCef(jsonProperty.Value), CefV8PropertyAttribute.None);
                    }
                    return plainObject;
                }
                case JTokenType.Array:
                {
                    var jArray = (JArray) jsonToken;
                    var arrayValue = CefV8Value.CreateArray(jArray.Count);
                    for (var i = 0; i < jArray.Count; i++)
                    {
                        arrayValue.SetValue(i, ToCef(jArray[i]));
                    }
                    return arrayValue;
                }
                case JTokenType.Integer:
                {
                    var jsonValue = (JValue) jsonToken;
                    if (jsonValue.Value is long)
                    {
                        return CefV8Value.CreateDouble((double) jsonToken);
                    }
                    return CefV8Value.CreateInt((int) jsonToken);
                }
                case JTokenType.Float:
                    return CefV8Value.CreateDouble((double) jsonToken);
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.String:
                    return CefV8Value.CreateString((string) jsonToken);
                case JTokenType.Boolean:
                    return CefV8Value.CreateBool((bool) jsonToken);
                case JTokenType.Null:
                    return CefV8Value.CreateNull();
                case JTokenType.Undefined:
                    return CefV8Value.CreateUndefined();
                case JTokenType.Date:
                    return CefV8Value.CreateDate((DateTime) jsonToken);
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.TimeSpan:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Constructor:
                case JTokenType.None:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static JToken ToJToken(CefV8Value cefV8Value)
        {
            if (!cefV8Value.IsNull && !cefV8Value.IsUndefined)
            {
                if (cefV8Value.IsBool)
                {
                    return new JValue(cefV8Value.GetBoolValue());
                }
                // CEF doesn't have a long data type so they are treated as doubles
                // To avoid overflow scenarios, IsDouble must be checked before IsInt because flags will be true
                if (cefV8Value.IsDouble || cefV8Value.IsInt || cefV8Value.IsUInt)
                {
                    return new JValue(cefV8Value.GetDoubleValue());
                }

                if (cefV8Value.IsString)
                {
                    return new JValue(cefV8Value.GetStringValue());
                }

                if (cefV8Value.IsDate)
                {
                    return new JValue(cefV8Value.GetDateValue());
                }
                if (cefV8Value.IsArray)
                {
                    var array = new JArray();
                    var v8ArrayLength = cefV8Value.GetArrayLength();
                    for (var v8ArrayIndex = 0; v8ArrayIndex < v8ArrayLength; ++v8ArrayIndex)
                    {
                        var elementToken = ToJToken(cefV8Value.GetValue(v8ArrayIndex));
                        array.Add(elementToken);
                    }
                    return array;
                }
                if (cefV8Value.IsObject)
                {
                    return JObjectToWrappedJToken(cefV8Value);
                }
            }
            return new JValue((object) null);
        }

        private static JToken JObjectToWrappedJToken(CefV8Value cefV8Value)
        {
            var jObjectWrapper = new JObject();
            var jObjectPayload = new JObject();
            var adapter = V8PluginAdapter.FromCefObject(cefV8Value);
            if (adapter != null)
            {
                // Send JObject that defines a reference to the plugin
                jObjectWrapper.Add("__type__", "reference");
                jObjectPayload.Add("id", adapter.Plugin.Descriptor.PluginId);
                jObjectWrapper.Add("__payload__", jObjectPayload);
            }
            else
            {
                // Not a plugin, serialize all fields to a structure in json
                var keys = cefV8Value.GetKeys();
                foreach (var fieldKey in keys)
                {
                    var value = cefV8Value.GetValue(fieldKey);
                    if (!value.IsFunction)
                    {
                        var fieldToken = ToJToken(value);
                        jObjectWrapper.Add(fieldKey, fieldToken);
                    }
                }
            }

            return jObjectWrapper;
        }

        public static JArray ToJArray(CefV8Value[] cefV8Values)
        {
            JArray jsonArray = null;
            if (cefV8Values != null)
            {
                jsonArray = new JArray();
                foreach (var cefV8Value in cefV8Values)
                {
                    jsonArray.Add(ToJToken(cefV8Value));
                }
            }
            return jsonArray;
        }
    }
}