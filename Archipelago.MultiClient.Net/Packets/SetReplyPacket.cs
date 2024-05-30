using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
