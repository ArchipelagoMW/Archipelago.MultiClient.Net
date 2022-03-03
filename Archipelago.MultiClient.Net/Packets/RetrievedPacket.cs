using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RetrievedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Retrieved;

        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }
    }
}
