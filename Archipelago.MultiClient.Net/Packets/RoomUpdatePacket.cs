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
	/// <summary>
	/// Sent when there is a need to update information about the present game session,
	/// All arguments for this packet are nullable, only the properties that are not null contain changes.
	/// </summary>
	public class RoomUpdatePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.RoomUpdate;

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("password")]
        public bool? Password { get; set; }

        [JsonProperty("permissions")]
        public Dictionary<string, Permissions> Permissions { get; set; } = new Dictionary<string, Permissions>();

        [JsonProperty("hint_cost")]
        public int? HintCostPercentage { get; set; }

        [JsonProperty("location_check_points")]
        public int? LocationCheckPoints { get; set; }

        [JsonProperty("players")]
        public NetworkPlayer[] Players { get; set; }

        [JsonProperty("hint_points")]
        public int? HintPoints { get; set; }

        [JsonProperty("checked_locations")]
        public long[] CheckedLocations { get; set; }
    }
}
