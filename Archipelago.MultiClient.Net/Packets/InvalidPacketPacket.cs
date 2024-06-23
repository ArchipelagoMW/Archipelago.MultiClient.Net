using Archipelago.MultiClient.Net.Enums;


#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

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
