using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetReplyPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.SetReply;

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public JToken Value { get; set; }

        [JsonProperty("default")]
        public JToken DefaultValue { get; set; }

        [JsonProperty("original_value")]
        public JToken OriginalValue { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("want_reply")]
        public bool WantReply { get; set; }

        public Guid? Reference { get; set; }
    }
}
