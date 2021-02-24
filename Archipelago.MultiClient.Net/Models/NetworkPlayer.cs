using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Models
{
    public struct NetworkPlayer
    {
        [JsonProperty("team")]
        public int Team { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
