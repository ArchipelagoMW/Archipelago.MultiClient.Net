using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Models
{
    public class DataPackage
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("games")]
        public Dictionary<string, GameData> Games { get; set; } = new Dictionary<string, GameData>();
    }
}
