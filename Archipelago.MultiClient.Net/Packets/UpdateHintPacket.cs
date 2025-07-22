using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class UpdateHintPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.UpdateHint;
        
        [JsonProperty("player")]
        public int Player { get; set; }
        
        [JsonProperty("location")]
        public long Location { get; set; }
        
        [JsonProperty("status")]
        public HintStatus Status { get; set; }
    }
}
