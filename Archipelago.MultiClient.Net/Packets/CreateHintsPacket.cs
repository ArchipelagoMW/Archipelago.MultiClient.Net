using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class CreateHintsPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.CreateHints;

        [JsonProperty("locations")]
        public long[] Locations { get; set; }

        [JsonProperty("player")]
        public int Player { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
