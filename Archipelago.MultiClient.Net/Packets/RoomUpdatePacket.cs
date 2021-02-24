using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClient.Net.Packets
{
    public class RoomUpdatePacket
    {
        [JsonProperty("hint_points")]
        public int HintPoints { get; set; }
    }
}
