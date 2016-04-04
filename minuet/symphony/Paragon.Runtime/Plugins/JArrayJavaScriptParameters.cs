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
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines translation between parameters received from a remote process and the C# parameters for the plugin method being invoked.
    /// </summary>
    public class JArrayJavaScriptParameters : IJavaScriptParameters
    {
        private readonly JArray _parameters;

        public JArrayJavaScriptParameters(JArray parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Convert the parameter values from the caller into native C# objects that match the supplied parameter definitions.
        /// </summary>
        /// <param name="parameterInfos">
        /// The parameter definitions for the method to be invoked.
        /// </param>
        /// <param name="pluginManager"></param>
        /// <returns>
        /// Native C# objects matching <param name="parameterInfos"></param>
        /// </returns>
        /// <remarks>
        /// The method will be called by <see cref="JavaScriptPlugin.InvokeFunction"/> on the calling thread, *before* dispatching to a background thread for execution.
        /// </remarks>
        public object[] GetConvertedParameters(ParameterInfo[] parameterInfos, IPluginManager pluginManager)
        {
            // TODO: ensure sufficient values have been provided (factoring in parameters with default values)

            var arguments = new object[parameterInfos.Length];
            for (var parameterInfoIndex = 0; parameterInfoIndex < parameterInfos.Length; ++parameterInfoIndex)
            {
                var currentParamInfo = parameterInfos[parameterInfoIndex];
                var paramType = currentParamInfo.ParameterType;

                // Check if the method takes more arguments than were supplied.
                if (parameterInfoIndex >= _parameters.Count)
                {
                    // If the arg is a params array type, supply an empty array of the specified type.
                    if (currentParamInfo.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0)
                    {
                        arguments[parameterInfoIndex] = Array.CreateInstance(paramType.GetElementType(), 0);

                        // Break because a params array has to be the final parameter.
                        break;
                    }

                    // Fallback to a default values.
                    if (paramType.IsValueType)
                    {
                        // Set to default for value types.

                        var defaultValue = (currentParamInfo.DefaultValue != null && !(currentParamInfo.DefaultValue is DBNull)) ? currentParamInfo.DefaultValue : Activator.CreateInstance(paramType);
                        arguments[parameterInfoIndex] = defaultValue;
                    }
                    else
                    {
                        // Set to default for objects.
                        var defaultValue = (currentParamInfo.DefaultValue != null && !(currentParamInfo.DefaultValue is DBNull)) ? currentParamInfo.DefaultValue : null;
                        arguments[parameterInfoIndex] = defaultValue;
                    }

                    continue;
                }

                // Get JToken for the current parameter.
                var parameterToken = _parameters[parameterInfoIndex];

                // Check if the current parameter is a params array.
                if (currentParamInfo.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0)
                {
                    // Create a params array of the specified type.
                    var elemType = paramType.GetElementType();
                    var paramArray = Array.CreateInstance(elemType, _parameters.Count - parameterInfos.Length + 1);

                    // Add the rest of the args to the params array.
                    var idx = 0;
                    for (var i = parameterInfoIndex; i < _parameters.Count; ++i)
                    {
                        // Add the converted value to the params array.
                        paramArray.SetValue(ConvertParamValue(_parameters[i], elemType, pluginManager), idx++);
                    }

                    // Add the param array to the args array.
                    arguments[parameterInfoIndex] = paramArray;

                    // Break because a params array has to be the final parameter.
                    break;
                }

                // Add the converted value to the args array.
                arguments[parameterInfoIndex] = ConvertParamValue(parameterToken, paramType, pluginManager);
            }

            return arguments;
        }

        private object ConvertParamValue(JToken parameterToken, Type parameterType, IPluginManager pluginManager)
        {
            // If the token is a JValue, extract the value differently based on whether
            // an underlying value exists or not.
            var jvalue = parameterToken as JValue;
            if (jvalue != null)
            {
                return jvalue.Value != null ? jvalue.ToObject(parameterType) : null;

                // No value exists (null).
            }

            return UnwrapParamValue(parameterToken, parameterType, pluginManager);
        }

        private object UnwrapParamValue(JToken parameterToken, Type parameterType, IPluginManager pluginManager)
        {
            var defaultValue = new Func<object>(() => parameterToken.ToObject(parameterType));
            var jobject = parameterToken as JObject;

            if (jobject != null)
            {
                if (jobject.Type == JTokenType.Object)
                {
                    if (parameterType == typeof (object))
                    {
                        return defaultValue();
                    }

                    if (parameterType == typeof (string))
                    {
                        // If the parameter is a JSON object, but the parameter type is string return the raw JSON.
                        return jobject.ToString();
                    }

                    JToken typeValue, payload;
                    if (!jobject.TryGetValue("__type__", out typeValue) || !jobject.TryGetValue("__payload__", out payload))
                    {
                        return defaultValue();
                    }

                    var payloadJObj = payload as JObject;
                    if (payloadJObj == null)
                    {
                        return defaultValue();
                    }

                    if (payloadJObj.Type != JTokenType.Object)
                    {
                        return defaultValue();
                    }

                    if ((string) typeValue != "reference")
                    {
                        return defaultValue();
                    }

                    JToken dynamicPluginGuid;
                    if (!payloadJObj.TryGetValue("id", out dynamicPluginGuid))
                    {
                        return defaultValue();
                    }

                    var dynamicPlugin = pluginManager.GetLocalPlugin((string) dynamicPluginGuid);
                    if (dynamicPlugin != null)
                    {
                        return dynamicPlugin.NativeObject;
                    }

                    throw new NullReferenceException("Failed to map Javascript parameter to native dynamic plugin object ");
                }
            }

            var jarray = parameterToken as JArray;
            if (jarray != null)
            {
                if (jarray.Type != JTokenType.Array)
                {
                    return defaultValue();
                }

                if (parameterType == typeof (object))
                {
                    return defaultValue();
                }

                var elementType = parameterType.GetElementType();
                if (elementType == null)
                {
                    if (!typeof (JArray).IsAssignableFrom(parameterType) && !typeof (Array).IsAssignableFrom(parameterType))
                    {
                        return parameterType == typeof (string) ? jarray.ToString() : null;
                    }

                    if (typeof (JArray).IsAssignableFrom(parameterType))
                    {
                        return defaultValue();
                    }
                }

                elementType = elementType ?? typeof (object);
                var nativeArray = Array.CreateInstance(elementType, jarray.Count);
                for (var i = 0; i < jarray.Count; i++)
                {
                    nativeArray.SetValue(Convert.ChangeType(UnwrapParamValue(jarray[i], elementType, pluginManager), elementType), i);
                }

                return nativeArray;
            }

            return defaultValue();
        }
    }
}