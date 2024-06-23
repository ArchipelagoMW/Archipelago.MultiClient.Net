using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;


#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

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
        public NetworkPlayer[] Players { get; set; }

        [JsonProperty("missing_locations")]
        public long[] MissingChecks { get; set; }
        [JsonProperty("checked_locations")]
        public long[] LocationsChecked { get; set; }
        [JsonProperty("slot_data")]
        public Dictionary<string, object> SlotData { get; set; }
        [JsonProperty("slot_info")]
        public Dictionary<int, NetworkSlot> SlotInfo { get; set; }
        
        [JsonProperty("hint_points")]
        public int? HintPoints { get; set; }
    }
}
