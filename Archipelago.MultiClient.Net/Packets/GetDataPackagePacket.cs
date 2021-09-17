using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class GetDataPackagePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.GetDataPackage;

        [JsonProperty("exclusions")]
        public List<string> Exclusions { get; set; }
    }
}
