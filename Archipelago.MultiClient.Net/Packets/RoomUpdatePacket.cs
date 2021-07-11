using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RoomUpdatePacket : RoomInfoPacket
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.RoomUpdate;

        [JsonProperty("hint_points")]
        public int HintPoints { get; set; }
    }
}
