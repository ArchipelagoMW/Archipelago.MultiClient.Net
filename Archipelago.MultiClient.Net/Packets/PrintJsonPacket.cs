﻿using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class PrintJsonPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.PrintJSON;

        [JsonProperty("data")]
        public List<JsonMessagePart> Data { get; set; } = new List<JsonMessagePart>();

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public JsonMessageType? MessageType { get; set; }
    }

    public class ItemPrintJsonPacket : PrintJsonPacket
    {
        [JsonProperty("receiving")]
        public int ReceivingPlayer { get; set; }

        [JsonProperty("item")]
        public NetworkItem Item { get; set; }
    }

    public class HintPrintJsonPacket : ItemPrintJsonPacket
    {
        [JsonProperty("found")]
        public bool? Found { get; set; }
    }
}
