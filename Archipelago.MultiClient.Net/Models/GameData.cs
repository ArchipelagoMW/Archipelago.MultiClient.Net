#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Models
{
    public class GameData
    {
        [JsonProperty("location_name_to_id")]
        public Dictionary<string, long> LocationLookup { get; set; } = new Dictionary<string, long>();

        [JsonProperty("item_name_to_id")]
        public Dictionary<string, long> ItemLookup { get; set; } = new Dictionary<string, long>();

        [JsonProperty("version")]
        public int Version { get; set; }
    }
}
