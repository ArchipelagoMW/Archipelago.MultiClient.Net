using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class DataPackagePacket: ArchipelagoPacketBase
    {
        [JsonProperty("data")]
        public DataPackage DataPackage { get; set; }
    }
}
