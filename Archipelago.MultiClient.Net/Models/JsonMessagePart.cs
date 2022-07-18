using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using Oculus.Newtonsoft.Json.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#endif

namespace Archipelago.MultiClient.Net.Models
{
    public class JsonMessagePart
    {
        [JsonProperty("type")]
#if USE_OCULUS_NEWTONSOFT
        public string Type { get; set; }
#else
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public JsonMessagePartType? Type { get; set; }
#endif

        [JsonProperty("color")]
#if USE_OCULUS_NEWTONSOFT
        public string Color { get; set; }
#else
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public JsonMessagePartColor? Color { get; set; }
#endif

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("player")]
        public int? Player { get; set; }

        [JsonProperty("flags")]
        public ItemFlags? Flags { get; set; }
    }
}