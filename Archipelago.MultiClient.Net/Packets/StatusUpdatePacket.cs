using MultiClient.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class StatusUpdatePacket: ArchipelagoPacketBase
    {
        [JsonProperty("status")]
        public ArchipelagoClientState Status { get; set; }
    }
}
