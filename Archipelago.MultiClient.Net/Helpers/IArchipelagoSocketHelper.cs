using System;
using System.Collections.Generic;

#if !NET35
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Delegates for the events that the IArchipelagoSocketHelper can raise
	/// </summary>
    public class ArchipelagoSocketHelperDelagates
    {
		/// <summary>
		/// Handler for recieved and sucsesfully parsed packages
		/// </summary>
		/// <param name="packet">the archipelago packet</param>
        public delegate void PacketReceivedHandler(ArchipelagoPacketBase packet);
		/// <summary>
		/// Handler for packets published to the websocket, called before the packet is handled by the server
		/// </summary>
		/// <param name="packets"></param>
        public delegate void PacketsSentHandler(ArchipelagoPacketBase[] packets);
		/// <summary>
		/// Handler for error on the socket or during parsing of the packets
		/// </summary>
		/// <param name="e">the exception that occured</param>
		/// <param name="message">the error that occured</param>
        public delegate void ErrorReceivedHandler(Exception e, string message);
		/// <summary>
		/// Handler for when the underlaying socket connection is closed
		/// </summary>
		/// <param name="reason">the close reason</param>
        public delegate void SocketClosedHandler(string reason);
		/// <summary>
		/// Handler for when the underlaying socket connection is opened to the archipelago server
		/// </summary>
		public delegate void SocketOpenedHandler();
    }

    /// <summary>
    /// Provides access to the underlying websocket, so send or receive messages
    /// </summary>
	public interface IArchipelagoSocketHelper
    {
	    /// <summary>
	    /// Handler for recieved and sucsesfully parsed packages
	    /// </summary>
		event ArchipelagoSocketHelperDelagates.PacketReceivedHandler PacketReceived;
        /// <summary>
        /// Handler for packets published to the websocket, called before the packet is handled by the server
        /// </summary>
		event ArchipelagoSocketHelperDelagates.PacketsSentHandler PacketsSent;
        /// <summary>
        /// Handler for error on the socket or during parsing of the packets
        /// </summary>
		event ArchipelagoSocketHelperDelagates.ErrorReceivedHandler ErrorReceived;
        /// <summary>
        /// Handler for when the underlaying socket connection is closed
        /// </summary>
		event ArchipelagoSocketHelperDelagates.SocketClosedHandler SocketClosed;
        /// <summary>
        /// Handler for when the underlaying socket connection is opened to the archipelago server
        /// </summary>
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