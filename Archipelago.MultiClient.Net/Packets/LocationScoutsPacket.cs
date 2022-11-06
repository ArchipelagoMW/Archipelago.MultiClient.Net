using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class LocationScoutsPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.LocationScouts;

        [JsonProperty("locations")]
        public long[] Locations { get; set; }

        [JsonProperty("create_as_hint")]
        public bool CreateAsHint { get; set; }
    }
}
