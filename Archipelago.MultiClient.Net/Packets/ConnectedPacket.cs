using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class ConnectedPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Connected;

        [JsonProperty("team")]
        public int Team { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("players")]
        public List<NetworkPlayer> Players { get; set; }

        [JsonProperty("missing_locations")]
        public List<int> MissingChecks { get; set; }

        [JsonProperty("checked_locations")]
        public List<int> ItemsChecked { get; set; }

        [JsonProperty("slot_data")]
        public Dictionary<string, object> SlotData { get; set; }
    }
}
