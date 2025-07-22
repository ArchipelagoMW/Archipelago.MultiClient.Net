#if NET35 || NET40
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Extensions;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Authentication;

#if !NET35
using System.Threading.Tasks;
#endif

using WebSocketSharp;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class ArchipelagoSocketHelper : IArchipelagoSocketHelper
    {
	    const SslProtocols Tls13 = (SslProtocols)12288;
	    const SslProtocols Tls12 = (SslProtocols)3072;
		
		static readonly ArchipelagoPacketConverter Converter = new ArchipelagoPacketConverter();

        public event ArchipelagoSocketHelperDelagates.PacketReceivedHandler PacketReceived;
        public event ArchipelagoSocketHelperDelagates.PacketsSentHandler PacketsSent;
        public event ArchipelagoSocketHelperDelagates.ErrorReceivedHandler ErrorReceived;
        public event ArchipelagoSocketHelperDelagates.SocketClosedHandler SocketClosed;
        public event ArchipelagoSocketHelperDelagates.SocketOpenedHandler SocketOpened;

#if !NET35
        TaskCompletionSource<bool> connectAsyncTask;
        TaskCompletionSource<bool> disconnectAsyncTask;
#endif

        /// <summary>
        ///     The URL of the host that the socket is connected to.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        ///     Returns true if the socket believes it is connected to the host.
        ///     Does not emit a ping to determine if the connection is stable.
        /// </summary>
        public bool Connected => webSocket != null &&
	        (webSocket.ReadyState == WebSocketState.Open || webSocket.ReadyState == WebSocketState.Closing);

        internal WebSocket webSocket;

        internal ArchipelagoSocketHelper(Uri hostUrl)
        {
	        Uri = hostUrl;
        }

		WebSocket CreateWebSocket(Uri uri)
        {
	        var socket = new WebSocket(uri.ToString());

	        if (socket.IsSecure)
		        socket.SslConfiguration.EnabledSslProtocols = Tls12 | Tls13;

	        socket.OnMessage += OnMessageReceived;
	        socket.OnError += OnError;
	        socket.OnClose += OnClose;
	        socket.OnOpen += OnOpen;

	        return socket;
        }

		/// <summary>
		///     Initiates a connection to the host.
		/// </summary>
		public void Connect() => ConnectToProvidedUri(Uri);

		void ConnectToProvidedUri(Uri uri)
		{
			if (uri.Scheme != "unspecified")
			{
				try
				{
					webSocket = CreateWebSocket(uri);
					webSocket.Connect();
				}
				catch (Exception e)
				{
					OnError(e);
				}
			}
			else
			{
				var errors = new List<Exception>();
				try
				{
					try
					{
						ConnectToProvidedUri(uri.AsWss());
					}
					catch (Exception e)
					{
						errors.Add(e);
						throw;
					}

					if (webSocket.IsAlive)
						return;

					try
					{
						ConnectToProvidedUri(uri.AsWs());
					}
					catch (Exception e)
					{
						errors.Add(e);
						throw;
					}
				}
				catch
				{
					try
					{
						ConnectToProvidedUri(uri.AsWs());
					}
					catch (Exception e)
					{
						errors.Add(e);
						
						OnError(new AggregateException(errors));
					}
				}
			}
		}
		
        /// <summary>
        ///     Disconnect from the host.
        /// </summary>
        public void Disconnect()
        {
            if (webSocket != null && webSocket.IsAlive)
                webSocket.Close();
        }

#if NET35
        /// <summary>
        ///     Disconnect from the host asynchronously.
        ///     Handle the <see cref="SocketClosed"/> event to add a callback.
        /// </summary>
        public void DisconnectAsync()
        {
            if (webSocket != null && webSocket.IsAlive)
                webSocket.CloseAsync();
        }
#else
        /// <summary>
        ///     Initiates a connection to the host synchronously.
        /// </summary>
        public Task ConnectAsync()
        {
            connectAsyncTask = new TaskCompletionSource<bool>();

			Task.Factory.StartNew(() =>
			{
				try
				{
					Connect();

					connectAsyncTask.TrySetResult(true);
				}
				catch (Exception e)
				{
					connectAsyncTask.TrySetException(e);
				}
			});

			return connectAsyncTask.Task;
        }

        /// <summary>
        ///     Disconnect from the host synchronously.
        /// </summary>
        public Task DisconnectAsync()
        {
            disconnectAsyncTask = new TaskCompletionSource<bool>();

            if (webSocket != null && webSocket.IsAlive)
                webSocket.CloseAsync();
            else
                disconnectAsyncTask.TrySetResult(false);

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
        public void SendPacket(ArchipelagoPacketBase packet) => SendMultiplePackets(packet);

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
        public void SendMultiplePackets(params ArchipelagoPacketBase[] packets)
        {
            if (webSocket != null && webSocket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                webSocket.Send(packetAsJson);

                if (PacketsSent != null)
                    PacketsSent(packets);
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
        public void SendPacketAsync(ArchipelagoPacketBase packet, Action<bool> onComplete = null) => 
	        SendMultiplePacketsAsync(new List<ArchipelagoPacketBase> { packet }, onComplete);

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
        public void SendMultiplePacketsAsync(List<ArchipelagoPacketBase> packets, Action<bool> onComplete = null) => 
	        SendMultiplePacketsAsync(onComplete, packets.ToArray());

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
                    PacketsSent(packets);
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
            var taskCompletionSource = new TaskCompletionSource<bool>();

            if (webSocket != null && webSocket.IsAlive)
            {
                var packetAsJson = JsonConvert.SerializeObject(packets);
                webSocket.SendAsync(packetAsJson, success => {
                    if (!success)
                        taskCompletionSource.TrySetException(new Exception("Failed to send packets async"));
                    else
                        taskCompletionSource.TrySetResult(true);
                });

                if (PacketsSent != null)
                    PacketsSent(packets);
            }
            else
            {
                taskCompletionSource.TrySetException(new ArchipelagoSocketClosedException());
            }

            return taskCompletionSource.Task;
        }
#endif

        void OnOpen(object sender, EventArgs e)
        {
#if !NET35
            if (connectAsyncTask != null)
                connectAsyncTask.TrySetResult(true);
#endif
            
            if (SocketOpened != null)
                SocketOpened();
        }

        void OnClose(object sender, CloseEventArgs e)
        {
#if !NET35
            if (disconnectAsyncTask != null)
                disconnectAsyncTask.TrySetResult(true);
#endif
			if (Uri.Scheme == "unspecified" && sender == webSocket && webSocket.Url.Scheme == "wss")
				return; //we ignore the first connection failure for unspecified protocol

            if (SocketClosed != null)
                SocketClosed(e.Reason);
        }

        void OnMessageReceived(object sender, MessageEventArgs e)
        {
	        if (!e.IsText || PacketReceived == null) return;

	        List<ArchipelagoPacketBase> packets = null;

	        try
	        {
		        packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(e.Data, Converter);
	        }
	        catch (Exception exception)
	        {
		        OnError(exception);
	        }
			
			if (packets != null)
				foreach (var packet in packets)
			        PacketReceived(packet);
        }

        void OnError(object sender, ErrorEventArgs e)
        {
            if (ErrorReceived != null)
                ErrorReceived(e.Exception, e.Message);
        }

        void OnError(Exception e)
        {
	        if (ErrorReceived != null)
		        ErrorReceived(e, e.Message);
	    }
	}
}
#endif