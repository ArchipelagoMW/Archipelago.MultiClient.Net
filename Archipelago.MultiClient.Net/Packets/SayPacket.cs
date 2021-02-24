using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class SayPacket: ArchipelagoPacketBase
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
