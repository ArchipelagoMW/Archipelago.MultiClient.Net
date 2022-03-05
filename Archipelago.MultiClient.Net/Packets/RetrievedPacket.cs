using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RetrievedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Retrieved;

        [JsonProperty("keys")]
        public Dictionary<string, JToken> Data { get; set; }
    }
}
