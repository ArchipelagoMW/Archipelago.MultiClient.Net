using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Archipelago.MultiClient.Net
{
    [Serializable]
    public class ArchipelagoPacketBase
    {
        [JsonProperty("cmd")]
        public ArchipelagoPacketType PacketType { get; set; }
    }
}
