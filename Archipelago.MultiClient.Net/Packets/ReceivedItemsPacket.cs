using MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class ReceivedItemsPacket: ArchipelagoPacketBase
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("items")]
        public List<NetworkItem> Items { get; set; }
    }
}
