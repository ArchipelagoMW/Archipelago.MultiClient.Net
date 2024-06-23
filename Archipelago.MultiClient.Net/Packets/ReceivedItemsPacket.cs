using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;


#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif
namespace Archipelago.MultiClient.Net.Packets
{
	public class ReceivedItemsPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ReceivedItems;

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("items")]
        public NetworkItem[] Items { get; set; }
    }
}
