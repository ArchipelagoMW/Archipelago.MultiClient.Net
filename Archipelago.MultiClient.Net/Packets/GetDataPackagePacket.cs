using Archipelago.MultiClient.Net.Enums;

namespace Archipelago.MultiClient.Net.Packets
{
    public class GetDataPackagePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.GetDataPackage;
    }
}
