using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Archipelago.MultiClient.Net.Models
{
    public class JsonMessagePart
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public JsonMessagePartType? Type { get; set; }

        [JsonProperty("color")]
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public JsonMessagePartColor? Color { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("player")]
        public int? Player { get; set; }

        [JsonProperty("flags")]
        public ItemFlags? Flags { get; set; }

        [JsonProperty("hint_status")]
        public HintStatus? HintStatus { get; set; }
	}
}