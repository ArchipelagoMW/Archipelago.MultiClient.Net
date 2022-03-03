using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Set;

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("default")]
        public object DefaultValue { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("want_reply")]
        public bool WantReply { get; set; }
    }
}
