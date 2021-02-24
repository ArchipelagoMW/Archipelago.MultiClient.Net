using MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class PrintJsonPacket: ArchipelagoPacketBase
    {
        [JsonProperty("data")]
        public List<JsonMessagePart> Data { get; set; }
    }
}
