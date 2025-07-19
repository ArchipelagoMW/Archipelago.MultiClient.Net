using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectionRefusedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectionRefused;

        [JsonProperty("errors", ItemConverterType = typeof(AttemptingStringEnumConverter))]
        public ConnectionRefusedError[] Errors { get; set; }
    }
}
