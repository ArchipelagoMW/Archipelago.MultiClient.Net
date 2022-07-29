using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class PrintPacket : ArchipelagoPacketBase, IPrintJsonPacket
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Print;

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonIgnore]
        public JsonMessagePart[] Data => new []{ new JsonMessagePart { Text = Text }};
    }
}
