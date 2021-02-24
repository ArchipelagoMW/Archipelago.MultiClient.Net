using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class LocationScoutsPacket: ArchipelagoPacketBase
    {
        [JsonProperty("locations")]
        public List<int> Locations { get; set; }
    }
}
