using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Archipelago.MultiClient.Net.Packets
{
    public interface IPrintJsonPacket
    {
        JsonMessagePart[] Data { get; }
    }

    public class PrintJsonPacket : ArchipelagoPacketBase, IPrintJsonPacket
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
}
