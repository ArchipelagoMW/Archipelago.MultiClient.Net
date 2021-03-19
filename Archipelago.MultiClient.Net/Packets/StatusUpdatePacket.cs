using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class StatusUpdatePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.StatusUpdate;

        [JsonProperty("status")]
        public ArchipelagoClientState Status { get; set; }
    }
}
