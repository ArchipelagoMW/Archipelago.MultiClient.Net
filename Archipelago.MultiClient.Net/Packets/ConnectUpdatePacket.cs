using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectUpdatePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectUpdate;

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonProperty("items_handling")]
        public ItemsHandlingFlags? ItemsHandling { get; set; }
    }
}
