using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class ConnectPacket: ArchipelagoPacketBase
    {
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("version")]
        public Version Version { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }
}
