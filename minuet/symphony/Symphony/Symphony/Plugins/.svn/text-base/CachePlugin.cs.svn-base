using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony.cache", IsBrowserSide = true)]
    public class CachePlugin
    {
        private readonly Dictionary<string, JObject> cache = new Dictionary<string, JObject>();

        [JavaScriptPluginMember]
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

        [JavaScriptPluginMember]
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
