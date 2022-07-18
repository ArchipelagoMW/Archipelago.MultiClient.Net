using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectionRefusedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectionRefused;

        [JsonProperty("errors", ItemConverterType = typeof(StringEnumConverter))]
        public ConnectionRefusedError[] Errors { get; set; }
    }
}
