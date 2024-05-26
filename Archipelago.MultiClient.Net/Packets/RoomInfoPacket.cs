using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RoomInfoPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.RoomInfo;

        [JsonProperty("version")]
        public NetworkVersion Version { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("password")]
        public bool Password { get; set; }

        [JsonProperty("permissions")]
        public Dictionary<string, Permissions> Permissions { get; set; }

        [JsonProperty("hint_cost")]
        public int HintCostPercentage { get; set; }

        [JsonProperty("location_check_points")]
        public int LocationCheckPoints { get; set; }

        [JsonProperty("players")]
        public NetworkPlayer[] Players { get; set; }

        [JsonProperty("games")]
        public string[] Games { get; set; }

        [JsonProperty("datapackage_checksums")]
        public Dictionary<string, string> DataPackageChecksums { get; set; }

		[JsonProperty("seed_name")]
        public string SeedName { get; set; }

        [JsonProperty("time")]
        public double Timestamp { get; set; }
    }
}
