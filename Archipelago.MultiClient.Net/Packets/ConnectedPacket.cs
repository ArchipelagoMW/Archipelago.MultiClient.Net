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
        public List<NetworkPlayer> Players { get; set; } = new List<NetworkPlayer>();

        [JsonProperty("missing_locations")]
        public List<long> MissingChecks { get; set; } = new List<long>();

        [JsonProperty("checked_locations")]
        public List<long> LocationsChecked { get; set; } = new List<long>();

        [JsonProperty("slot_data")]
        public Dictionary<string, object> SlotData { get; set; } = new Dictionary<string, object>();
    }
}
