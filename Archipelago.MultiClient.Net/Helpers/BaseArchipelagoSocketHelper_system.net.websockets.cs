#if NET45 || NETSTANDARD2_0 || NET6_0
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Websocket agnostic version of the Socket helper, allows a different socket class to be used for testing
	/// </summary>
	/// <typeparam name="T">The type of WebSocket to use</typeparam>
    public class BaseArchipelagoSocketHelper<T> where T : WebSocket
	{
		// ReSharper disable once StaticMemberInGenericType
		static readonly ArchipelagoPacketConverter Converter = new ArchipelagoPacketConverter();

		/// <summary>
		/// Handler for recieved and sucsesfully parsed packages
		/// </summary>
		public event ArchipelagoSocketHelperDelagates.PacketReceivedHandler PacketReceived;
		/// <summary>
		/// Handler for packets published to the websocket, called before the packet is handled by the server
		/// </summary>
		public event ArchipelagoSocketHelperDelagates.PacketsSentHandler PacketsSent;
		/// <summary>
		/// Handler for error on the socket or during parsing of the packets
		/// </summary>
		public event ArchipelagoSocketHelperDelagates.ErrorReceivedHandler ErrorReceived;
		/// <summary>
		/// Handler for when the underlaying socket connection is closed
		/// </summary>
		public event ArchipelagoSocketHelperDelagates.SocketClosedHandler SocketClosed;
		/// <summary>
		/// Handler for when the underlaying socket connection is opened to the archipelago server
		/// </summary>
		public event ArchipelagoSocketHelperDelagates.SocketOpenedHandler SocketOpened;

		readonly BlockingCollection<Tuple<ArchipelagoPacketBase, TaskCompletionSource<bool>>> sendQueue =
	        new BlockingCollection<Tuple<ArchipelagoPacketBase, TaskCompletionSource<bool>>>();

		/// <summary>
        ///     Returns true if the socket believes it is connected to the host.
        ///     Does not emit a ping to determine if the connection is stable.
        /// </summary>
        public bool Connected => Socket.State == WebSocketState.Open || Socket.State == WebSocketState.CloseReceived;

        internal T Socket;
        readonly int bufferSize;

        /// <summary>
        ///     Signals both background loops to stop. Cancelled by <see cref="DisconnectAsync"/>.
        /// </summary>
        /// <remarks>
        ///     Without it neither loop can ever end, and both keep the whole session graph
        ///     alive: an async state machine that is awaiting is rooted by the runtime, and
        ///     through the socket helper it references the session's helpers, the session
        ///     itself, and its copy of the data package.
        /// </remarks>
        readonly CancellationTokenSource shutdown = new CancellationTokenSource();

        internal BaseArchipelagoSocketHelper(T socket, int bufferSize = 1024)
        {
	        Socket = socket;
	        this.bufferSize = bufferSize;
        }

		internal void StartPolling()
        {
	        if (SocketOpened != null)
		        SocketOpened();

			_ = Task.Run(PollingLoop);
	        _ = Task.Run(SendLoop);
        }

		async Task PollingLoop()
        {
            var buffer = new byte[bufferSize];

            while (Socket.State == WebSocketState.Open && !shutdown.IsCancellationRequested)
            {
                string message = null;

                try
                {
                    message = await ReadMessageAsync(buffer, shutdown.Token);
                }
                catch (OperationCanceledException)
                {
                    // DisconnectAsync asked us to stop. This is the expected way out, not an
                    // error: reporting it would surface a failure on every clean disconnect.
                    return;
                }
                catch (Exception e)
                {
                    OnError(e);
                }

                OnMessageReceived(message);

            }
        }

        async Task SendLoop()
        {
            while (Socket.State == WebSocketState.Open && !shutdown.IsCancellationRequested)
            {
                try
                {
                    await HandleSendBuffer(shutdown.Token);
                }
                catch (OperationCanceledException)
                {
                    // The send queue was completed by DisconnectAsync: nothing left to send.
                    return;
                }
                catch (Exception e)
                {
                    OnError(e);
                }

                await Task.Delay(20);
			}
        }

        async Task<string> ReadMessageAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            using (var readStream = new MemoryStream(buffer.Length))
            {
	            WebSocketReceiveResult result;
	            do
	            {
		            result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

		            if (result.MessageType == WebSocketMessageType.Close)
		            {
			            try
			            {
				            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
			            }
			            catch
			            {
				            // ignore failure to close when a close is requested as the connection might already be dropped
			            }

			            OnSocketClosed();
		            }
		            else
		            {
						readStream.Write(buffer, 0, result.Count);
		            }
	            } while (!result.EndOfMessage);

				return Encoding.UTF8.GetString(readStream.ToArray());
			}
        }

        /// <summary>
        ///     Disconnect from the host asynchronously.
        ///     Handle the <see cref="SocketClosed"/> event to add a callback.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (shutdown.IsCancellationRequested)
                return;

            try
            {
                if (Socket.State == WebSocketState.Open)
                {
                    // Bounded: CloseAsync waits for the server's close frame, and a server
                    // that never answers would otherwise hang this call forever.
                    using (var closing = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                        await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                            "Closure requested by client", closing.Token);
                }
            }
            catch
            {
                // Best effort. The connection may already be gone, and a failure to say
                // goodbye must not stop us from releasing everything below.
            }

            // Order matters: the loops are released first, then the socket, then listeners
            // are told. Disposing before cancelling would make the pending read fail with a
            // disposal error rather than end quietly.
            shutdown.Cancel();
            sendQueue.CompleteAdding();
            Socket.Dispose();

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
        public void SendPacket(ArchipelagoPacketBase packet) => SendMultiplePackets(new List<ArchipelagoPacketBase> { packet });

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
        public void SendMultiplePackets(List<ArchipelagoPacketBase> packets) => SendMultiplePackets(packets.ToArray());

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
        public void SendMultiplePackets(params ArchipelagoPacketBase[] packets) => SendMultiplePacketsAsync(packets).Wait();

        /// <summary>
        ///     Send a single <see cref="ArchipelagoPacketBase"/> derived packet asynchronously.
        /// </summary>
        /// <param name="packet">
        ///     The packet to send to the server.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public Task SendPacketAsync(ArchipelagoPacketBase packet) => SendMultiplePacketsAsync(new List<ArchipelagoPacketBase> { packet });

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
        public Task SendMultiplePacketsAsync(List<ArchipelagoPacketBase> packets) => SendMultiplePacketsAsync(packets.ToArray());

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
                sendQueue.Add(new Tuple<ArchipelagoPacketBase, TaskCompletionSource<bool>>(packet, task));

            return task.Task;
        }

        async Task HandleSendBuffer(CancellationToken cancellationToken)
        {
            var packetList = new List<ArchipelagoPacketBase>();
            var tasks = new List<TaskCompletionSource<bool>>();

            // Take() blocks the calling thread while the queue is empty. Without a token it
            // never returns, because the queue is only ever completed by DisconnectAsync:
            // every socket that is opened and left alone costs one thread pool thread for
            // the lifetime of the process.
            var firstPacketTuple = sendQueue.Take(cancellationToken);
            packetList.Add(firstPacketTuple.Item1);
            tasks.Add(firstPacketTuple.Item2);
            while (sendQueue.TryTake(out var packetTuple))
            {
                packetList.Add(packetTuple.Item1);
                tasks.Add(packetTuple.Item2);
            }

            if (!packetList.Any())
                return;

            if (Socket.State != WebSocketState.Open)
                throw new ArchipelagoSocketClosedException();
            
            var packets = packetList.ToArray();
            
            var packetAsJson = JsonConvert.SerializeObject(packets);
            var messageBuffer = Encoding.UTF8.GetBytes(packetAsJson);
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / bufferSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (bufferSize * i);
                var count = bufferSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                    count = messageBuffer.Length - offset;

                await Socket.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), 
	                WebSocketMessageType.Text, lastMessage, CancellationToken.None);
            }

            foreach (var task in tasks)
                task.TrySetResult(true);

            OnPacketSend(packets);
        }

        void OnPacketSend(ArchipelagoPacketBase[] packets)
        {
            try
            {
                if (PacketsSent != null)
                    PacketsSent(packets);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        void OnSocketClosed()
        {
            try
            {
                if (SocketClosed != null)
                    SocketClosed("");
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        void OnMessageReceived(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message) && PacketReceived != null)
                {
	                List<ArchipelagoPacketBase> packets = null;

					try
	                {
		                packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, Converter);
					}
	                catch (Exception exception)
	                {
						OnError(exception);
	                }

                    if (packets == null)
                        return;

                    foreach (var packet in packets)
                        PacketReceived(packet);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

		/// <summary>
		/// Error handler to call when an exception occurs, it will trigger the socket's ErrorRecieved handler
		/// </summary>
		/// <param name="e">the exception that occured</param>
        protected void OnError(Exception e)
        {
            try
            {
                if (ErrorReceived != null)
                    ErrorReceived(e, e.Message);
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