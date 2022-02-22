﻿using Archipelago.MultiClient.Net.Converters;
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
        public Dictionary<string, Permissions> Permissions { get; set; } = new Dictionary<string, Permissions>();

        [JsonProperty("hint_cost")]
        public int HintCost { get; set; }

        [JsonProperty("location_check_points")]
        public int LocationCheckPoints { get; set; }

        [JsonProperty("players")]
        public List<NetworkPlayer> Players { get; set; } = new List<NetworkPlayer>();

        [JsonProperty("games")]
        public List<string> Games { get; set; } = new List<string>();

        [JsonProperty("datapackage_version")]
        public int DataPackageVersion { get; set; }

        [JsonProperty("datapackage_versions")]
        public Dictionary<string, int> DataPackageVersions { get; set; } = new Dictionary<string, int>();

        [JsonProperty("seed_name")]
        public string SeedName { get; set; }

        [JsonProperty("time")]
        public double Timestamp { get; set; }
    }
}
