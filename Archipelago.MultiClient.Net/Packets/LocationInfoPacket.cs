using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;

#endif
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class LocationInfoPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.LocationInfo;

        [JsonProperty("locations")]
        public NetworkItem[] Locations { get; set; }
    }
}
