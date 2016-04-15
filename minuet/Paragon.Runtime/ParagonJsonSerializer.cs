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

using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Paragon.Runtime
{
    public static class ParagonJsonSerializer
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCaseContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new JsonConverter[] {new StringEnumConverter()},
            Formatting = Formatting.Indented
        };

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public static string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, Settings);
        }

        private class CamelCaseContractResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonProperty CreateProperty(
                MemberInfo member, MemberSerialization memberSerialization)
            {
                var result = base.CreateProperty(member, memberSerialization);

                var attributes = (JsonPropertyAttribute[]) member
                    .GetCustomAttributes(typeof (JsonPropertyAttribute), true);

                foreach (var attribute in attributes.Where(
                    attribute => !string.IsNullOrEmpty(attribute.PropertyName)))
                {
                    result.PropertyName = attribute.PropertyName;
                }

                return result;
            }
        }
    }
}