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