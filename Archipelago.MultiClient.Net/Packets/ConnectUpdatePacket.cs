using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectUpdatePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.ConnectUpdate;

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("items_handling")]
        public ItemsHandlingFlags? ItemsHandling { get; set; }
    }
}
