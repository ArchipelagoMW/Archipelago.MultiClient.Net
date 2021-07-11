using Archipelago.MultiClient.Net.Enums;
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
        public List<JsonMessagePart> Data { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public JsonMessageType? MessageType { get; set; }

        [JsonProperty("receiving")]
        public int ReceivingPlayer { get; set; }

        [JsonProperty("item")]
        public NetworkItem Item { get; set; }
    }
}
