using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class PrintJsonPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.PrintJSON;

        [JsonProperty("data")]
        public JsonMessagePart[] Data { get; set; }

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

    public class CountdownPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("countdown")]
	    public int RemainingSeconds { get; set; }
	}
}
