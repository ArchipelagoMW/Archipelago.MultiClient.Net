using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class ConnectionRefusedPacket: ArchipelagoPacketBase
    {
        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}
