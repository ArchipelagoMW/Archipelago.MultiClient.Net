using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RoomInfoPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.RoomInfo;

        [JsonProperty("version")]
        [JsonConverter(typeof(NamedTupleInterchangeConverter))]
        public Version Version { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("password")]
        public bool Password { get; set; }

        [JsonProperty("permissions")]
        public Dictionary<string, Permissions> Permissions { get; set; }

        [JsonProperty("hint_cost")]
        public int HintCost { get; set; }

        [JsonProperty("location_check_points")]
        public int LocationCheckPoints { get; set; }

        [JsonProperty("players")]
        public NetworkPlayer[] Players { get; set; }

        [JsonProperty("games")]
        public string[] Games { get; set; }

        [JsonProperty("datapackage_version")]
        public int DataPackageVersion { get; set; }

        [JsonProperty("datapackage_versions")]
        public Dictionary<string, int> DataPackageVersions { get; set; }

        [JsonProperty("seed_name")]
        public string SeedName { get; set; }

        [JsonProperty("time")]
        public double Timestamp { get; set; }
    }
}
