using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class DataPackagePacket : ArchipelagoPacketBase
    {
        [JsonProperty("data")]
        public DataPackage DataPackage { get; set; }
    }
}
