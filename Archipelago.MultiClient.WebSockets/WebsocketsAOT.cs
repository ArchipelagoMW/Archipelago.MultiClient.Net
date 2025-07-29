using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Archipelago.MultiClient.WebSockets
{
	public class WebsocketsAOT
	{
		delegate void OnSocketOpenedCallbackDelegate();
		delegate void OnSocketClosedCallbackDelegate();
		delegate void OnErrorCallbackDelegate(IntPtr message, IntPtr stack);
		delegate void OnMessageCallbackDelegate(IntPtr message);

		static int BufferSize = 1024;

		static ClientWebSocket Socket;

		static OnSocketOpenedCallbackDelegate OnSocketOpenedCallback;
		static OnSocketClosedCallbackDelegate OnSockedClosedCallback;
		static OnMessageCallbackDelegate OnMessageCallback;
		static OnErrorCallbackDelegate OnErrorCallback;
		
		[UnmanagedCallersOnly(EntryPoint = "WebSockets.AOT.StartPolling")]
		public static void StartPolling(int bufferSize, IntPtr onSockedOpenedCallback, IntPtr onSockedClosedCallback, IntPtr onMessageCallback, IntPtr onErrorCallback)
		{
			OnSocketOpenedCallback();

			BufferSize = bufferSize;

			OnSocketOpenedCallback = Marshal.GetDelegateForFunctionPointer<OnSocketOpenedCallbackDelegate>(onSockedOpenedCallback);
			OnSockedClosedCallback = Marshal.GetDelegateForFunctionPointer<OnSocketClosedCallbackDelegate>(onSockedClosedCallback);
			OnMessageCallback = Marshal.GetDelegateForFunctionPointer<OnMessageCallbackDelegate>(onMessageCallback);
			OnErrorCallback = Marshal.GetDelegateForFunctionPointer<OnErrorCallbackDelegate>(onErrorCallback);
			
			_ = Task.Run(PollingLoop);
		}

		[UnmanagedCallersOnly(EntryPoint = "WebSockets.AOT.GetSocketState")]
		public static int GetSocketState() => (int)Socket.State;

		[UnmanagedCallersOnly(EntryPoint = "WebSockets.AOT.SendMessage")]
		public static void SendMessage(IntPtr packetsAsJsonPtr)
		{
			try
			{
				var packetsAsJson = Marshal.PtrToStringAnsi(packetsAsJsonPtr);
				if (packetsAsJson == null)
					return;

				var t = Task.Run(() => SendMessageAsync(packetsAsJson));
				t.Wait();
			}
			catch (Exception e)
			{
				RaiseException(e);
			}
		}

		public static async Task SendMessageAsync(string packetsAsJson)
		{
			var messageBuffer = Encoding.UTF8.GetBytes(packetsAsJson);
			var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / BufferSize);

			for (var i = 0; i < messagesCount; i++)
			{
				var offset = (BufferSize * i);
				var count = BufferSize;
				var lastMessage = ((i + 1) == messagesCount);

				if ((count * (i + 1)) > messageBuffer.Length)
					count = messageBuffer.Length - offset;

				await Socket.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count),
					WebSocketMessageType.Text, lastMessage, CancellationToken.None);
			}
		}

		static async Task PollingLoop()
		{
			try
			{
				var buffer = new byte[BufferSize];

				while (Socket.State == WebSocketState.Open)
				{
					try
					{
						var message = await ReadMessageAsync(buffer);

						OnMessageCallback?.Invoke(Marshal.StringToHGlobalAnsi(message));
					}
					catch (Exception e)
					{
						var errorMessage = Marshal.StringToHGlobalAnsi(e.Message);
						var stacktrace = Marshal.StringToHGlobalAnsi(e.StackTrace);

						OnErrorCallback(errorMessage, stacktrace);
					}
				}
			}
			catch (Exception e)
			{
				RaiseException(e);
			}
		}

		static async Task<string> ReadMessageAsync(byte[] buffer)
		{
			using var readStream = new MemoryStream(buffer.Length);

			WebSocketReceiveResult result;
			do
			{
				result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

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

					OnSockedClosedCallback();
				}
				else
				{
					readStream.Write(buffer, 0, result.Count);
				}
			} while (!result.EndOfMessage);

			return Encoding.UTF8.GetString(readStream.ToArray());
		}

		static void RaiseException(Exception e)
		{
			if (e is AggregateException ae)
			{
				foreach (var innerException in ae.InnerExceptions)
				{
					RaiseExceptionInternal(innerException);
				}
			}
			else
			{
				RaiseExceptionInternal(e);
			}
		}

		static void RaiseExceptionInternal(Exception e)
		{
			var errorMessage = Marshal.StringToHGlobalAnsi(e.Message);
			var stacktrace = Marshal.StringToHGlobalAnsi(e.StackTrace);

			OnErrorCallback(errorMessage, stacktrace);
		}
	}
}
