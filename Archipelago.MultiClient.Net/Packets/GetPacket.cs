using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
    public class GetPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Get;

        [JsonProperty("keys")]
        public string[] Keys { get; set; }
    }
}
