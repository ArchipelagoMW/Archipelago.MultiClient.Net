using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class LocationScoutsPacket: ArchipelagoPacketBase
    {
        [JsonProperty("locations")]
        public List<int> Locations { get; set; }
    }
}
