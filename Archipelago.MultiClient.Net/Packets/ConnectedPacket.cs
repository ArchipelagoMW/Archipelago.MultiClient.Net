using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectedPacket : ArchipelagoPacketBase
    {
        [JsonProperty("team")]
        public int Team { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("players")]
        public List<NetworkPlayer> Players { get; set; }

        [JsonProperty("missing_checks")]
        public List<int> MissingChecks { get; set; }

        [JsonProperty("items_checked")]
        public List<int> ItemsChecked { get; set; }
    }
}
