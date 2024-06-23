using System;
using System.Collections.Generic;

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Models
{
    public class GameData
    {
        [JsonProperty("location_name_to_id")]
        public Dictionary<string, long> LocationLookup { get; set; } = new Dictionary<string, long>();

        [JsonProperty("item_name_to_id")]
        public Dictionary<string, long> ItemLookup { get; set; } = new Dictionary<string, long>();

		[Obsolete("use Checksum instead")]
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("checksum")]
        public string Checksum { get; set; }
	}
}
