using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ReceivedItemsPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ReceivedItems;

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("items")]
        public List<NetworkItem> Items { get; set; }
    }
}
