using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net.Models
{
    public class GameData
    {
        [JsonProperty("location_name_to_id")]
        public Dictionary<string, int> LocationLookup { get; set; }

        [JsonProperty("item_name_to_id")]
        public Dictionary<string, int> ItemLookup { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
    }
}
