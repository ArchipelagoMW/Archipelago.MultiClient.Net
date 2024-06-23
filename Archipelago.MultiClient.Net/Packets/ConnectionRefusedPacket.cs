using Archipelago.MultiClient.Net.Enums;


#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
	public class ConnectionRefusedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectionRefused;

        [JsonProperty("errors", ItemConverterType = typeof(StringEnumConverter))]
        public ConnectionRefusedError[] Errors { get; set; }
    }
}
