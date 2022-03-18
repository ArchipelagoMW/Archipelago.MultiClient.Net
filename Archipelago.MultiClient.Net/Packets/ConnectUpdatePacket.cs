using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectUpdatePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectUpdate;

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("items_handling")]
        public ItemsHandlingFlags? ItemsHandling { get; set; }
    }
}
