using Archipelago.MultiClient.Net.Enums;

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetReplyPacket : SetPacket
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.SetReply;

        [JsonProperty("value")]
        public JToken Value { get; set; }

        [JsonProperty("original_value")]
        public JToken OriginalValue { get; set; }
	}
}
