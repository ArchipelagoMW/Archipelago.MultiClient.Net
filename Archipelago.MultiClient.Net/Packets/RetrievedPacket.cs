using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json.Linq;
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
#endif
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
