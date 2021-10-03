using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net.Models
{
    public class DataPackage
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("games")]
        public Dictionary<string, GameData> Games { get; set; }
    }
}
