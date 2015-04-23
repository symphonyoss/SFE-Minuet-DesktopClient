using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Symphony.NativeServices
{
    public class CacheNativeService
    {
        private readonly Dictionary<string, JObject> cache = new Dictionary<string, JObject>();

        public JObject GetValue(string key)
        {
            JObject json;
            lock (this.cache)
            {
                this.cache.TryGetValue(key, out json);
            }
            if (json == null)
            {
                json = new JObject();
                json[key] = "undefined";
            }

            return json;
        }

        public void SetValue(string key, string value)
        {
            JObject json = new JObject();
            json[key] = value;

            lock (this.cache)
            {
                this.cache[key] = json;
            }
        }
    }
}
