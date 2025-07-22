using Archipelago.MultiClient.Net.Enums;


#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
	public class ConnectionRefusedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectionRefused;


#if NET6_0_OR_GREATER
		[JsonProperty("errors")]
#else
        [JsonProperty("errors", ItemConverterType = typeof(AttemptingStringEnumConverter))]
#endif
		public ConnectionRefusedError[] Errors { get; set; }
    }
}
