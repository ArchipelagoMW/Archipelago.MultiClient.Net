using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

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
        public List<int> GroupMembers { get; set; }
    }
}