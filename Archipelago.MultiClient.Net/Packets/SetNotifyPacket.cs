using Archipelago.MultiClient.Net.Enums;


#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif
namespace Archipelago.MultiClient.Net.Packets
{
	public class SetNotifyPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.SetNotify;

        [JsonProperty("keys")]
        public string[] Keys { get; set; }
    }
}
