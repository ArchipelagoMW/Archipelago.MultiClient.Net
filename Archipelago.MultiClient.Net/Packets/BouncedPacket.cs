using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Archipelago.MultiClient.Net.Packets
{
    public class BouncedPacket : BouncePacket
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Bounced;
    }
}
