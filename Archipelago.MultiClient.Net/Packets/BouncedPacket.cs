using Archipelago.MultiClient.Net.Enums;

namespace Archipelago.MultiClient.Net.Packets
{
    public class BouncedPacket : BouncePacket
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Bounced;
    }
}
