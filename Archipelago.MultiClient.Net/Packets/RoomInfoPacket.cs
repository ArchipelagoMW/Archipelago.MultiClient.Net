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

        [JsonProperty("forfeit_mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ForfeitModeType ForfeitMode { get; set; }

        [JsonProperty("remaining_mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RemainingModeType RemainingMode { get; set; }

        [JsonProperty("hint_cost")]
        public int HintCost { get; set; }

        [JsonProperty("location_check_points")]
        public int LocationCheckPoints { get; set; }

        [JsonProperty("players")]
        public List<NetworkPlayer> Players { get; set; }

        [JsonProperty("datapackage_version")]
        public int DataPackageVersion { get; set; }

        [JsonProperty("datapackage_versions")]
        public Dictionary<string, int> DataPackageVersions { get; set; }

        [JsonProperty("seed_name")]
        public string SeedName { get; set; }
    }
}
