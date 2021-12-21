using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
    public class DataPackagePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.DataPackage;

        [JsonProperty("data")]
        public DataPackage DataPackage { get; set; }
    }
}
