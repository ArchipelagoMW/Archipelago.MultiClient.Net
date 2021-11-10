using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class InvalidPacketPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.InvalidPacket;
        
        [JsonProperty("type")]
        public InvalidPacketErrorType ErrorType { get; set; }

        [JsonProperty("text")]
        public string ErrorText { get; set; }

        [JsonProperty("original_cmd")]
        public ArchipelagoPacketType OriginalCmd { get; set; }
    }
}
