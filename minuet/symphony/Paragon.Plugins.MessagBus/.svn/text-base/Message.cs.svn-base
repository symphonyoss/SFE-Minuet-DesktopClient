using Newtonsoft.Json;

namespace Paragon.Plugins.MessageBus
{
    [JsonObject]
    public class Message
    {
        [JsonProperty("address")]
        public string Topic { get; set; }
        [JsonProperty("message")]
        public object Data { get; set; }
        [JsonProperty("rid")]
        public string Rid { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("registered")]
        public bool Registered { get; set; }
    }
}