using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif
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
