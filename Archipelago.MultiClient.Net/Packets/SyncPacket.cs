using Archipelago.MultiClient.Net.Enums;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SyncPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Sync;
    }
}
