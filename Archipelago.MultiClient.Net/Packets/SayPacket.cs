using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
    public class SayPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Say;

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
