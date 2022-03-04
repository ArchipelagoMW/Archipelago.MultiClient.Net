using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Set;

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("default")]
        public object DefaultValue { get; set; }

        [JsonProperty("operations")]
        public OperationSpecification[] Operation { get; set; }

        [JsonProperty("want_reply")]
        public bool WantReply { get; set; }
    }
}
