using MultiClient.Net.Enums;
using MultiClient.Net.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MultiClient.Net
{
    [Serializable]
    public class ArchipelagoPacketBase
    {
        [JsonProperty("cmd")]
        public ArchipelagoPacketType PacketType { get; set; }
    }
}
