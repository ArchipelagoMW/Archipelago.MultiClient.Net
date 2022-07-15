#if NET35 || NET40
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#if !NET35
using System.Threading.Tasks;
#endif

using WebSocketSharp;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ArchipelagoSocketHelper : IArchipelagoSocketHelper
    {
        public event ArchipelagoSocketHelperDelagates.PacketReceivedHandler PacketReceived;
        public event ArchipelagoSocketHelperDelagates.PacketsSentHandler PacketsSent;
        public event ArchipelagoSocketHelperDelagates.ErrorReceivedHandler ErrorReceived;
        public event ArchipelagoSocketHelperDelagates.SocketClosedHandler SocketClosed;
        public event ArchipelagoSocketHelperDelagates.SocketOpenedHandler SocketOpened;

#if !NET35
        private TaskCompletionSource<bool> connectAsyncTask;
        private TaskCompletionSource<bool> disconnectAsyncTask;

#endif

        /// <summary>
        ///     The URL of the host that the socket is connected to.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        ///     Returns true if the socket believes it is connected to the host.
        ///     Does not emit a ping to determine if the connection is stable.
        /// </summary>
        public bool Connected { get => webSocket.ReadyState == WebSocketState.Open || webSocket.ReadyState == WebSocketState.Closing; }

        private readonly WebSocket webSocket;

        internal ArchipelagoSocketHelper(Uri hostUrl)
        {
            Uri = hostUrl;
            webSocket = new WebSocket(Uri.ToString());
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
        ///     Disconnect from the host.
        /// </summary>
        public void Disconnect()
        {
            if (webSocket.IsAlive)
            {
                webSocket.Close();
            }
        }

#if NET35
        /// <summary>
        ///     Initiates a connection to the host asynchronously.
        ///     Handle the <see cref="SocketOpened"/> event to add a callback.
        /// </summary>
        public void ConnectAsync()
        {
            webSocket.ConnectAsync();
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
#else
        /// <summary>
        ///     Initiates a connection to the host synchronously.
        /// </summary>
        public Task ConnectAsync()
        {
            connectAsyncTask = new TaskCompletionSource<bool>();

            webSocket.ConnectAsync();

            return connectAsyncTask.Task;
        }

        /// <summary>
        ///     Disconnect from the host synchronously.
        /// </summary>
        public Task DisconnectAsync()
        {
            disconnectAsyncTask = new TaskCompletionSource<bool>();

            if (webSocket.IsAlive)
            {
                webSocket.CloseAsync();
            }
            else
            {
                disconnectAsyncTask.SetResult(false);
            }

            return disconnectAsyncTask.Task;
        }
#endif

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

#if NET35
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
#else
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
            var taskCompletionSource = new TaskCompletionSource<bool>();

            if (webSocket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                webSocket.SendAsync(packetAsJson, success => {
                    if (!success)
                        taskCompletionSource.SetException(new Exception("Failed to send packets async"));
                    else
                        taskCompletionSource.SetResult(true);
                });

                if (PacketsSent != null)
                {
                    PacketsSent(packets);
                }
            }
            else
            {
                taskCompletionSource.SetException(new ArchipelagoSocketClosedException());
            }

            return taskCompletionSource.Task;
        }
#endif

        private void OnOpen(object sender, EventArgs e)
        {
#if !NET35
            connectAsyncTask.SetResult(true);
#endif
            
            if (SocketOpened != null)
            {
                SocketOpened();
            }
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
#if !NET35
            disconnectAsyncTask.SetResult(true);
#endif
            if (SocketClosed != null)
            {
                SocketClosed(e.Reason);
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
#endif