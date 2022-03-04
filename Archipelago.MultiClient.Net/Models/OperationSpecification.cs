using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Archipelago.MultiClient.Net.Models
{
    public class OperationSpecification
    {
        [JsonProperty("operation")]
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public Operation Operation;

        [JsonProperty("value")]
        public object Value { get; set; }
    }
}