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
#if !USE_OCULUS_NEWTONSOFT
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
#endif
        public JsonMessagePartType? Type { get; set; }

        [JsonProperty("color")]
#if !USE_OCULUS_NEWTONSOFT
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
#endif
        public JsonMessagePartColor? Color { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}