using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectionRefusedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectionRefused;

        [JsonProperty("errors", ItemConverterType = typeof(StringEnumConverter))]
        public ConnectionRefusedError[] Errors { get; set; }
    }
}
