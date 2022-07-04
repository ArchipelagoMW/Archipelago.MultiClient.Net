#if NETSTANDARD2_0
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ArchipelagoSocketHelper : IArchipelagoSocketHelper
    {
        public delegate void PacketReceivedHandler(ArchipelagoPacketBase packet);
        public event PacketReceivedHandler PacketReceived;

        public delegate void PacketsSentHandler(ArchipelagoPacketBase[] packets);
        public event PacketsSentHandler PacketsSent;

        public delegate void ErrorReceivedHandler(Exception e, string message);
        public event ErrorReceivedHandler ErrorReceived;

        public delegate void SocketClosedHandler();
        public event SocketClosedHandler SocketClosed;

        public event Action SocketOpened;

        /// <summary>
        ///     The URL of the host that the socket is connected to.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        ///     Returns true if the socket believes it is connected to the host.
        ///     Does not emit a ping to determine if the connection is stable.
        /// </summary>
        public bool Connected { get => webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived; }

        private readonly ClientWebSocket webSocket;

        internal ArchipelagoSocketHelper(Uri hostUri)
        {
            Uri = hostUri;
            webSocket = new ClientWebSocket();
        }

        /// <summary>
        ///     Initiates a connection to the host asynchronously.
        ///     Handle the <see cref="SocketOpened"/> event to add a callback.
        /// </summary>
        public async Task ConnectAsync()
        {
            await webSocket.ConnectAsync(Uri, CancellationToken.None);

            if (SocketOpened != null)
            {
                SocketOpened();
            }

            
        }

        /// <summary>
        ///     Disconnect from the host asynchronously.
        ///     Handle the <see cref="SocketClosed"/> event to add a callback.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closure requested by client",
                CancellationToken.None);

            if (SocketClosed != null)
            {
                SocketClosed();
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
            SendMultiplePacketsAsync(packets).Wait();
        }

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet asynchronously.
        /// </summary>
        /// <param name="packet">
        ///     The packet to send to the server.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public Task SendPacketAsync(ArchipelagoPacketBase packet)
        {
            return SendMultiplePacketsAsync(new List<ArchipelagoPacketBase> { packet });
        }

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet asynchronously.
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
        public Task SendMultiplePacketsAsync(List<ArchipelagoPacketBase> packets)
        {
            return SendMultiplePacketsAsync(packets.ToArray());
        }

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet asynchronously.
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
        public async Task SendMultiplePacketsAsync(params ArchipelagoPacketBase[] packets)
        {
            if (!Connected)
            {
                await Task.FromException(new ArchipelagoSocketClosedException());
            }
            else
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                var bytes = Encoding.UTF8.GetBytes(packetAsJson);
                var data = new ArraySegment<byte>(bytes, 0, bytes.Length);

                await webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);

                if (PacketsSent != null)
                {
                    PacketsSent(packets);
                }
            }
        }

        /*
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
        */
    }
}
#endif

/*
    do
    {
        using (var socket = new ClientWebSocket())
            try
            {
                await socket.ConnectAsync(new Uri(Connection), CancellationToken.None);

                await Send(socket, "data");
                await Receive(socket);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");
            }
    } while (true); 


static async Task Receive(ClientWebSocket socket)
{
    var buffer = new ArraySegment<byte>(new byte[2048]);
    do
    {
        WebSocketReceiveResult result;
        using (var ms = new MemoryStream())
        {
            do
            {
                result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
                break;

            ms.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(ms, Encoding.UTF8))
                Console.WriteLine(await reader.ReadToEndAsync());
        }
    } while (true);
}
*/

