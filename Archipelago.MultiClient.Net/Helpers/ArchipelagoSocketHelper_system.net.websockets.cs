#if NETSTANDARD2_0 || NET6_0
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ArchipelagoSocketHelper : IArchipelagoSocketHelper
    {
        private const int PollingDelayInMilliseconds = 16;

        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        public event ArchipelagoSocketHelperDelagates.PacketReceivedHandler PacketReceived;
        public event ArchipelagoSocketHelperDelagates.PacketsSentHandler PacketsSent;
        public event ArchipelagoSocketHelperDelagates.ErrorReceivedHandler ErrorReceived;
        public event ArchipelagoSocketHelperDelagates.SocketClosedHandler SocketClosed;
        public event ArchipelagoSocketHelperDelagates.SocketOpenedHandler SocketOpened;

        private readonly ConcurrentQueue<Tuple<ArchipelagoPacketBase, TaskCompletionSource<bool>>> sendQueue
            = new ConcurrentQueue<Tuple<ArchipelagoPacketBase, TaskCompletionSource<bool>>>();

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
#if NET6_0
            webSocket.Options.DangerousDeflateOptions = new WebSocketDeflateOptions();
#endif
        }

        /// <summary>
        ///     Initiates a connection to the host synchronously.
        /// </summary>
        public void Connect()
        {
            ConnectAsync().Wait();
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

            _ = Task.Run(PollingLoop);
            _ = Task.Run(SendLoop);
        }

        private async Task PollingLoop()
        {
            var buffer = new byte[ReceiveChunkSize];

            while (webSocket.State == WebSocketState.Open)
            {
                string message = null;

                try
                {
                    message = await ReadMessageAsync(buffer);
                }
                catch (Exception e)
                {
                    OnError(e);
                }

                OnMessageReceived(message);

                await Task.Delay(PollingDelayInMilliseconds);
            }
        }

        private async Task SendLoop()
        {
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await HandleSendBuffer();
                }
                catch (Exception e)
                {
                    OnError(e);
                }

                await Task.Delay(PollingDelayInMilliseconds);
            }
        }

        private async Task<string> ReadMessageAsync(byte[] buffer)
        {
            var stringResult = new StringBuilder();

            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                    OnSocketClosed();
                }
                else
                {
                    stringResult.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
            } while (!result.EndOfMessage);

            return stringResult.ToString();
        }

        /// <summary>
        ///     Disconnect from the host synchronously.
        /// </summary>
        public void Disconnect()
        {
            DisconnectAsync().Wait();
        }

        /// <summary>
        ///     Disconnect from the host asynchronously.
        ///     Handle the <see cref="SocketClosed"/> event to add a callback.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closure requested by client",
                CancellationToken.None);

            OnSocketClosed();
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
        public Task SendMultiplePacketsAsync(params ArchipelagoPacketBase[] packets)
        {
            var task = new TaskCompletionSource<bool>();

            foreach (var packet in packets)
            {
                sendQueue.Enqueue(new Tuple<ArchipelagoPacketBase, TaskCompletionSource<bool>>(packet, task));
            }

            return task.Task;
        }

        private async Task HandleSendBuffer()
        {
            List<ArchipelagoPacketBase> packetList = new List<ArchipelagoPacketBase>();
            List<TaskCompletionSource<bool>> tasks = new List<TaskCompletionSource<bool>>();

            while (sendQueue.TryDequeue(out var packetTuple))
            {
                packetList.Add(packetTuple.Item1);
                tasks.Add(packetTuple.Item2);
            }

            if (!packetList.Any())
            {
                return;
            }

            if (webSocket.State != WebSocketState.Open)
            {
                throw new ArchipelagoSocketClosedException();
            }
            
            ArchipelagoPacketBase[] packets = packetList.ToArray();
            
            var packetAsJson = JsonConvert.SerializeObject(packets);
            var messageBuffer = Encoding.UTF8.GetBytes(packetAsJson);
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SendChunkSize * i);
                var count = SendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }

                await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, CancellationToken.None);
            }

            foreach (var task in tasks)
            {
                task.TrySetResult(true);
            }

            OnPacketSend(packets);
        }

        private void OnPacketSend(ArchipelagoPacketBase[] packets)
        {
            try
            {
                if (PacketsSent != null)
                {
                    PacketsSent(packets);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void OnSocketClosed()
        {
            try
            {
                if (SocketClosed != null)
                {
                    SocketClosed("");
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void OnMessageReceived(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message) && PacketReceived != null)
                {
                    var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, new ArchipelagoPacketConverter());
                    if (packets == null)
                        return;

                    foreach (var packet in packets)
                    {
                        PacketReceived(packet);
                    }
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void OnError(Exception e)
        {
            try
            {
                if (ErrorReceived != null)
                {
                    ErrorReceived(e, e.Message);
                }
            }
            catch (Exception innerError)
            {
                Console.Out.WriteLine(
                    $"Error occured during reporting of error" +
                    $"Outer Errror: {e.Message} {e.StackTrace}" +
                    $"Inner Errror: {innerError.Message} {innerError.StackTrace}");
            }
        }
    }
}
#endif