#if NET47 || NET48 || NET6_0
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class BaseArchipelagoSocketHelperFixture
	{
		[TestCase(@"[{ ""cmd"":""Say"", ""text"": ""some message"" }]", 100)]
		[TestCase(@"[{ ""cmd"":""Say"", ""text"": ""some message"" }]", 10)]
		public async Task should_read_message(string message, int bufferSize)
		{
			var sut = new BaseArchipelagoSocketHelper<TestWebSocket>(new TestWebSocket(message), bufferSize);

			Exception error = null;
			ArchipelagoPacketBase receivedPacket = null;
			
			sut.PacketReceived += packet => receivedPacket = packet;
			sut.ErrorReceived += (e, _) => error = e;

			sut.StartPolling();

			int maxRetries = 10;
			int retryCount = 0;
			while (receivedPacket == null && retryCount++ < maxRetries)
				await Task.Delay(100);

			var sayPacket = receivedPacket as SayPacket;

			Assert.That(error, Is.Null);
			Assert.That(sayPacket, Is.Not.Null);
			Assert.That(sayPacket.Text, Is.EqualTo("some message"));
		}

		[Test]
		public void should_throw_error_failed_parse()
		{
			//var sut = new BaseArchipelagoSocketHelper<TestWebSocket>(new TestWebSocket());
		}

	}

	class TestWebSocket : WebSocket
	{
		MemoryStream incommingBytes;

		public TestWebSocket(string inMessage)
		{
			incommingBytes = new MemoryStream(Encoding.UTF8.GetBytes(inMessage));
		}

		public override void Abort() { }

		public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) 
			=> Task.CompletedTask;

		public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) 
			=> Task.CompletedTask;

		public override void Dispose() { }

		public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> outBuffer, CancellationToken cancellationToken)
		{
			var readCount = await incommingBytes.ReadAsync(outBuffer.Array, 0, outBuffer.Count, cancellationToken);

			return new WebSocketReceiveResult(readCount, WebSocketMessageType.Text, incommingBytes.Position == incommingBytes.Length);
		}

		public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
			CancellationToken cancellationToken)
		{

			await Task.CompletedTask;
		}

		public override WebSocketCloseStatus? CloseStatus => null;
		public override string CloseStatusDescription => null;
		public override string SubProtocol => null;
		public override WebSocketState State => WebSocketState.Open;
	}
}
#endif
