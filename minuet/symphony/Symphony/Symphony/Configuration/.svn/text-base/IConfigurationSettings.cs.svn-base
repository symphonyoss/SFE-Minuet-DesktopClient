using Newtonsoft.Json.Linq;

namespace Symphony.Configuration
{
    public interface IConfigurationSettings
    {
        TEnum ConvertEnumValueOrDefault<TEnum>(string @namespace, string key, TEnum defaultValue)
            where TEnum : struct;

        T GetValueOrDefault<T>(string @namespace, string key, T defaultValue);

        TEnum GetEnumValueOrDefault<TEnum>(string @namespace, string key, TEnum defaultValue) 
            where TEnum : struct;

        void Write(string @namespace, JObject settings);
    }
}