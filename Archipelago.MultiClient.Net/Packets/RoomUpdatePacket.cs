using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RoomUpdatePacket : ArchipelagoPacketBase
    {
        [JsonProperty("hint_points")]
        public int HintPoints { get; set; }
    }
}
