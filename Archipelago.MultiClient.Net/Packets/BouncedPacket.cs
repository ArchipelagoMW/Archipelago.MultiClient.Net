using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Archipelago.MultiClient.Net.Packets
{
    public class BouncedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Bounced;

        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }
    }
}
