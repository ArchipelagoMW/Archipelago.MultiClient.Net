using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Packets
{
    public class RoomUpdatePacket : RoomInfoPacket
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.RoomUpdate;

        [JsonProperty("hint_points")]
        public int HintPoints { get; set; }

        [JsonProperty("checked_locations")]
        public int[] CheckedLocations { get; set; }
    }
}
