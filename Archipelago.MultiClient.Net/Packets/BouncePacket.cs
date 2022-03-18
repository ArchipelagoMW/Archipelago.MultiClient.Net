using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class BouncePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Bounce;

        [JsonProperty("games")]
        public List<string> Games { get; set; } = new List<string>();

        [JsonProperty("slots")]
        public List<int> Slots { get; set; } = new List<int>();

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }
    }
}
