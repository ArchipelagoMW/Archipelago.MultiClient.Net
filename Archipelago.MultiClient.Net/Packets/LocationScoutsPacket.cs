using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System.Collections.Generic;

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
