using System;
using System.Collections.Generic;

#if !NET35
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ArchipelagoSocketHelperDelagates
    {
        public delegate void PacketReceivedHandler(ArchipelagoPacketBase packet);
        public delegate void PacketsSentHandler(ArchipelagoPacketBase[] packets);
        public delegate void ErrorReceivedHandler(Exception e, string message);
        public delegate void SocketClosedHandler(string reason);
        public delegate void SocketOpenedHandler();
    }

    /// <summary>
    /// Provides access to the underlying websocket, so send or receive messages
    /// </summary>
	public interface IArchipelagoSocketHelper
    {
        event ArchipelagoSocketHelperDelagates.PacketReceivedHandler PacketReceived;
        event ArchipelagoSocketHelperDelagates.PacketsSentHandler PacketsSent;
        event ArchipelagoSocketHelperDelagates.ErrorReceivedHandler ErrorReceived;
        event ArchipelagoSocketHelperDelagates.SocketClosedHandler SocketClosed;
        event ArchipelagoSocketHelperDelagates.SocketOpenedHandler SocketOpened;

        Uri Uri { get; }

        bool Connected { get; }

        void SendPacket(ArchipelagoPacketBase packet);
        void SendMultiplePackets(List<ArchipelagoPacketBase> packets);
        void SendMultiplePackets(params ArchipelagoPacketBase[] packets);

#if NET35
		void Connect();
		void Disconnect();

		void SendPacketAsync(ArchipelagoPacketBase packet, Action<bool> onComplete = null);
        void SendMultiplePacketsAsync(List<ArchipelagoPacketBase> packets, Action<bool> onComplete = null);
        void SendMultiplePacketsAsync(Action<bool> onComplete = null, params ArchipelagoPacketBase[] packets);
#else
		Task ConnectAsync();
        Task DisconnectAsync();

        Task SendPacketAsync(ArchipelagoPacketBase packet);
        Task SendMultiplePacketsAsync(List<ArchipelagoPacketBase> packets);
        Task SendMultiplePacketsAsync(params ArchipelagoPacketBase[] packets);
#endif
	}
}