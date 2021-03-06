using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class LocationInfoPacket: ArchipelagoPacketBase
    {
        [JsonProperty("locations")]
        public List<NetworkItem> Locations { get; set; }
    }
}
