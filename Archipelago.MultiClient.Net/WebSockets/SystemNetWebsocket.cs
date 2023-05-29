using Archipelago.MultiClient.Net.Helpers;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.WebSockets
{
	class SystemNetWebsocket : IClientWebSocket
	{
		public event Action<string> OnMessageReceived;
		public event Action<Exception> OnErrorReceived;
		public event Action<string> OnSocketClosed;
		public event Action OnSocketOpened;

		const int ReceiveChunkSize = 1024;
		const int SendChunkSize = 1024;

		Uri uri;
		ClientWebSocket webSocket;

		public SystemNetWebsocket(Uri hostUri)
		{
			uri = hostUri;
			webSocket = new ClientWebSocket();
#if NET45
			var Tls13 = (SecurityProtocolType)12288;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | Tls13;
#endif
#if NET6_0
            webSocket.Options.DangerousDeflateOptions = new WebSocketDeflateOptions();
#endif
		}
		
		public bool Connected => webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived;

		public async Task ConnectAsync()
		{
			await webSocket.ConnectAsync(uri, CancellationToken.None);
		}

		public async Task DisconnectAsync()
		{
			await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closure requested by client", CancellationToken.None);
		}

		public async Task SendAsync(string messageJson)
		{
			var messageBuffer = Encoding.UTF8.GetBytes(messageJson);
			var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

			for (var i = 0; i < messagesCount; i++)
			{
				var offset = (SendChunkSize * i);
				var count = SendChunkSize;
				var lastMessage = ((i + 1) == messagesCount);

				if ((count * (i + 1)) > messageBuffer.Length)
					count = messageBuffer.Length - offset;

				await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count),
					WebSocketMessageType.Text, lastMessage, CancellationToken.None);
			}
		}
	}
}
