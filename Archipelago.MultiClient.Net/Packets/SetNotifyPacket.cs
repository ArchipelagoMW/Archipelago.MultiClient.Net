using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetNotifyPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.SetNotify;

        [JsonProperty("keys")]
        public string[] Keys { get; set; }
    }
}
