using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectionRefusedPacket : ArchipelagoPacketBase
    {
        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}
