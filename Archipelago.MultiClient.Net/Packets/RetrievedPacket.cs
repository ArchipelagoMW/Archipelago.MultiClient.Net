using Archipelago.MultiClient.Net.Enums;
using System.Collections.Generic;

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
	public class RetrievedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Retrieved;

        [JsonProperty("keys")]
        public Dictionary<string, JToken> Data { get; set; }
    }
}
