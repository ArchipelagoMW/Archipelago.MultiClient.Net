using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetNotifyPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.SetNotify;

        [JsonProperty("keys")]
        public string[] Keys { get; set; }
    }
}
