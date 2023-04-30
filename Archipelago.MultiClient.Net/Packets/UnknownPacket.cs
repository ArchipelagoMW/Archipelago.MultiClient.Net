using Archipelago.MultiClient.Net.Enums;

namespace Archipelago.MultiClient.Net.Packets
{
	/// <summary>
	/// Packet is used whenever the server sends us a packet that we didnt implement in the library
	/// </summary>
	class UnknownPacket : ArchipelagoPacketBase
	{
		public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Unknown;
	}
}
