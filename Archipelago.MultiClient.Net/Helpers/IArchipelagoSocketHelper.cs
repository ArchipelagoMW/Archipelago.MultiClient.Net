#if NET35
using System;
#else
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
    public interface IArchipelagoSocketHelper
    {
        event ArchipelagoSocketHelper.PacketReceivedHandler PacketReceived;

        void SendPacket(ArchipelagoPacketBase packet);

#if NET35
        void SendPacketAsync(ArchipelagoPacketBase packet, Action<bool> onComplete = null);
#elif NET40
        Task<bool> SendPacketAsync(ArchipelagoPacketBase packet);
#else
        Task SendPacketAsync(ArchipelagoPacketBase packet);
#endif
    }
}