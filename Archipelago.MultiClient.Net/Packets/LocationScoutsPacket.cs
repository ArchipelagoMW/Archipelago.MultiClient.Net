using Archipelago.MultiClient.Net.Enums;


#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
	public class LocationScoutsPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.LocationScouts;

        [JsonProperty("locations")]
        public long[] Locations { get; set; }

        [JsonProperty("create_as_hint")]
        public int CreateAsHint { get; set; }
    }
}
