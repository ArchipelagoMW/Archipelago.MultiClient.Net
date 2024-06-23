using System.Collections.Generic;

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Models
{
    public class DataPackage
    {
        [JsonProperty("games")]
        public Dictionary<string, GameData> Games { get; set; } = new Dictionary<string, GameData>();
    }
}
