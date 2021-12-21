using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
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
        public List<int> MissingChecks { get; set; } = new List<int>();

        [JsonProperty("checked_locations")]
        public List<int> LocationsChecked { get; set; } = new List<int>();

        [JsonProperty("slot_data")]
        public Dictionary<string, object> SlotData { get; set; } = new Dictionary<string, object>();
    }
}
