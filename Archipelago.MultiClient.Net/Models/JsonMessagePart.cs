using Newtonsoft.Json;

namespace MultiClient.Net.Models
{
    public class JsonMessagePart
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}