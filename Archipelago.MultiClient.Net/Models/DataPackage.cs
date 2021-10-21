using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net.Models
{
    public class DataPackage
    {
        [JsonProperty("lookup_any_location_id_to_name")]
        public Dictionary<int, string> LocationLookup { get; set; }

        [JsonProperty("lookup_any_item_id_to_name")]
        public Dictionary<int, string> ItemLookup { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("games")]
        public Dictionary<string, GameData> Games { get; set; }
    }
}
