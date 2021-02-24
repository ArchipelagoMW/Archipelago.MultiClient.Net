using MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class LocationInfoPacket: ArchipelagoPacketBase
    {
        [JsonProperty("locations")]
        public List<NetworkItem> Locations { get; set; }
    }
}
