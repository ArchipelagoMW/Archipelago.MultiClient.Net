using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class StatusUpdatePacket : ArchipelagoPacketBase
    {
        [JsonProperty("status")]
        public ArchipelagoClientState Status { get; set; }
    }
}
