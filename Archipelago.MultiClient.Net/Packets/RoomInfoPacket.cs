using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RoomInfoPacket : ArchipelagoPacketBase
    {
        [JsonProperty("version")]
        public Version Version { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("password")]
        public bool Password { get; set; }

        [JsonProperty("forfeit_mode")]
        public string ForfeitMode { get; set; }

        [JsonProperty("remaining_mode")]
        public string RemainingMode { get; set; }

        [JsonProperty("hint_cost")]
        public int HintCost { get; set; }

        [JsonProperty("location_check_points")]
        public int LocationCheckPoints { get; set; }

        [JsonProperty("players")]
        public List<NetworkPlayer> Players { get; set; }

        [JsonProperty("datapackage_version")]
        public int DataPackageVersion { get; set; }
    }
}
