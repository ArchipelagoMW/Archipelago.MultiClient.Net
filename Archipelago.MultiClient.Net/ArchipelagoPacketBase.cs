using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Archipelago.MultiClient.Net
{
    [Serializable]
    public class ArchipelagoPacketBase
    {
        [JsonProperty("cmd")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ArchipelagoPacketType PacketType { get; set; }
    }
}
