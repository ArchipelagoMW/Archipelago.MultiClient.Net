using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ReceivedItemsPacket : ArchipelagoPacketBase
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("items")]
        public List<NetworkItem> Items { get; set; }
    }
}
