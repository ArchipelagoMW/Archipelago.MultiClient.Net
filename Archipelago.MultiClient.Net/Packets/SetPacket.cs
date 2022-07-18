using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json.Linq;
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
#endif
using System;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Set;

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("default")]
        public JToken DefaultValue { get; set; }

        [JsonProperty("operations")]
        public OperationSpecification[] Operations { get; set; }

        [JsonProperty("want_reply")]
        public bool WantReply { get; set; }

        public Guid? Reference { get; set; }
    }
}
