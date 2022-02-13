using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    /// <summary>
    /// Sent when there is a need to update information about the present game session,
    /// All arguments for this packet are nullable, only the properties that are not null contain changes.
    /// </summary>
    public class RoomUpdatePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.RoomUpdate;

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("password")]
        public bool? Password { get; set; }

        [JsonProperty("permissions")]
        public Dictionary<string, Permissions> Permissions { get; set; } = new Dictionary<string, Permissions>();

        [JsonProperty("hint_cost")]
        public int? HintCost { get; set; }

        [JsonProperty("location_check_points")]
        public int? LocationCheckPoints { get; set; }

        [JsonProperty("players")]
        public List<NetworkPlayer> Players { get; set; } = new List<NetworkPlayer>();

        [JsonProperty("hint_points")]
        public int? HintPoints { get; set; }

        [JsonProperty("checked_locations")]
        public List<int> CheckedLocations { get; set; }
    }
}
