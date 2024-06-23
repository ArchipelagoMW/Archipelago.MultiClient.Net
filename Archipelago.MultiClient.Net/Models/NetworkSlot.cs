using Archipelago.MultiClient.Net.Enums;

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Models
{
    public struct NetworkSlot
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("game")]
        public string Game { get; set; }
        [JsonProperty("type")]
        public SlotType Type { get; set; }
        [JsonProperty("group_members")]
        public int[] GroupMembers { get; set; }
    }
}