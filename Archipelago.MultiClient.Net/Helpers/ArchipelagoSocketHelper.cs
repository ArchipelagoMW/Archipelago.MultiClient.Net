using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace Archipelago.MultiClient.Net.Helpers
{
    public interface IArchipelagoSocketHelper
    {
        event ArchipelagoSocketHelper.PacketReceivedHandler PacketReceived;

        void SendPacket(ArchipelagoPacketBase packet);

        void SendPacketAsync(ArchipelagoPacketBase packet, Action<bool> onComplete = null);
    }


    public class ArchipelagoSocketHelper : IArchipelagoSocketHelper
    {
        public delegate void PacketReceivedHandler(ArchipelagoPacketBase packet);
        public event PacketReceivedHandler PacketReceived;

        public delegate void PacketsSentHandler(ArchipelagoPacketBase[] packets);
        public event PacketsSentHandler PacketsSent;

        public delegate void ErrorReceivedHandler(Exception e, string message);
        public event ErrorReceivedHandler ErrorReceived;

        public delegate void SocketClosedHandler(CloseEventArgs e);
        public event SocketClosedHandler SocketClosed;

        public event Action SocketOpened;

        /// <summary>
        ///     The URL of the host that the socket is connected to.
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     Returns true if the socket believes it is connected to the host.
        ///     Does not emit a ping to determine if the connection is stable.
        /// </summary>
        public bool Connected { get => webSocket.ReadyState == WebSocketState.Open || webSocket.ReadyState == WebSocketState.Closing; }

        private readonly WebSocket webSocket;

        internal ArchipelagoSocketHelper(string hostUrl)
        {
            Url = hostUrl;
            webSocket = new WebSocket(hostUrl);
            webSocket.OnMessage += OnMessageReceived;
            webSocket.OnError += OnError;
            webSocket.OnClose += OnClose;
            webSocket.OnOpen += OnOpen;
        }

        /// <summary>
        ///     Initiates a connection to the host.
        /// </summary>
        public void Connect()
        {
            webSocket.Connect();
        }

        /// <summary>
        ///     Initiates a connection to the host asynchronously.
        ///     Handle the <see cref="SocketOpened"/> event to add a callback.
        /// </summary>
        public void ConnectAsync()
        {
            webSocket.ConnectAsync();
        }

        /// <summary>
        ///     Disconnect from the host.
        /// </summary>
        public void Disconnect()
        {
            if (webSocket.IsAlive)
            {
                webSocket.Close();
            }
        }

        /// <summary>
        ///     Disconnect from the host asynchronously.
        ///     Handle the <see cref="SocketClosed"/> event to add a callback.
        /// </summary>
        public void DisconnectAsync()
        {
            if (webSocket.IsAlive)
            {
                webSocket.CloseAsync();
            }
        }

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet.
        /// </summary>
        /// <param name="packet">
        ///     The packet to send to the server.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public void SendPacket(ArchipelagoPacketBase packet)
        {
            SendMultiplePackets(new List<ArchipelagoPacketBase> { packet });
        }

        /// <summary>
        ///     Send multiple <see cref="ArchipelagoPacketBase"/> derived packets.
        /// </summary>
        /// <param name="packets">
        ///     The packets to send to the server.
        /// </param>
        /// <remarks>
        ///     The packets will be sent in the order they are provided in the list.
        /// </remarks>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public void SendMultiplePackets(List<ArchipelagoPacketBase> packets)
        {
            SendMultiplePackets(packets.ToArray());
        }

        /// <summary>
        ///     Send multiple <see cref="ArchipelagoPacketBase"/> derived packets.
        /// </summary>
        /// <param name="packets">
        ///     The packets to send to the server.
        /// </param>
        /// <remarks>
        ///     The packets will be sent in the order they are provided as arguments.
        /// </remarks>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public void SendMultiplePackets(params ArchipelagoPacketBase[] packets)
        {
            if (webSocket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                webSocket.Send(packetAsJson);

                if (PacketsSent != null)
                {
                    PacketsSent(packets);
                }
            }
            else
            {
                throw new ArchipelagoSocketClosedException();
            }
        }

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet asynchronously.
        /// </summary>
        /// <param name="onComplete">
        ///     A callback function to run after the send is complete.
        ///     The <see cref="bool"/> argument for the callback indicates whether the send was successful.
        /// </param>
        /// <param name="packet">
        ///     The packet to send to the server.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public void SendPacketAsync(ArchipelagoPacketBase packet, Action<bool> onComplete = null)
        {
            SendMultiplePacketsAsync(new List<ArchipelagoPacketBase> { packet }, onComplete);
        }

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet asynchronously.
        /// </summary>
        /// <param name="onComplete">
        ///     A callback function to run after the send is complete.
        ///     The <see cref="bool"/> argument for the callback indicates whether the send was successful.
        /// </param>
        /// <param name="packets">
        ///     The packets to send to the server.
        /// </param>
        /// <remarks>
        ///     The packets will be sent in the order they are provided in the list.
        /// </remarks>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public void SendMultiplePacketsAsync(List<ArchipelagoPacketBase> packets, Action<bool> onComplete = null)
        {
            SendMultiplePacketsAsync(onComplete, packets.ToArray());
        }

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet asynchronously.
        /// </summary>
        /// <param name="onComplete">
        ///     A callback function to run after the send is complete.
        ///     The <see cref="bool"/> argument for the callback indicates whether the send was successful.
        /// </param>
        /// <param name="packets">
        ///     The packets to send to the server.
        /// </param>
        /// <remarks>
        ///     The packets will be sent in the order they are provided as arguments.
        /// </remarks>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public void SendMultiplePacketsAsync(Action<bool> onComplete = null, params ArchipelagoPacketBase[] packets)
        {
            if (webSocket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                webSocket.SendAsync(packetAsJson, onComplete);

                if (PacketsSent != null)
                {
                    PacketsSent(packets);
                }
            }
            else
            {
                throw new ArchipelagoSocketClosedException();
            }
        }

        private void OnOpen(object sender, EventArgs e)
        {
            if (SocketOpened != null)
            {
                SocketOpened();
            }
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            if (SocketClosed != null)
            {
                SocketClosed(e);
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.IsText && PacketReceived != null)
            {
                var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(e.Data, new ArchipelagoPacketConverter());
                foreach (var packet in packets)
                {
                    PacketReceived(packet);
                }
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            if (ErrorReceived != null)
            {
                ErrorReceived(e.Exception, e.Message);
            }
        }
    }
}
