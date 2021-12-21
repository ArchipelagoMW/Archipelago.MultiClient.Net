using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
    public class RoomUpdatePacket : RoomInfoPacket
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.RoomUpdate;

        [JsonProperty("hint_points")]
        public int HintPoints { get; set; }
    }
}
