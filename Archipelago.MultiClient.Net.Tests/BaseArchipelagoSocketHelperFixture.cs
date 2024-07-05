#if NET47 || NET48 || NET6_0
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class BaseArchipelagoSocketHelperFixture
	{
		[TestCase("some message", 100, Description = "Buffer bigger than message")]
		[TestCase("some message", 10, Description = "Buffer smaller than message")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊",  10, Description = "UTF8 complex character breaking A")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊",  11, Description = "UTF8 complex character breaking B")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊",  12, Description = "UTF8 complex character breaking C")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊",  13, Description = "UTF8 complex character breaking D")]
		public async Task Should_read_message_from_websocket_and_parse_archipelago_package(string message, int bufferSize)
		{
			var sayPacketJson = @"[{ ""cmd"":""Say"", ""text"": ""$MESSAGE"" }]".Replace("$MESSAGE", message);
			var sut = new BaseArchipelagoSocketHelper<TestWebSocket>(new TestWebSocket(sayPacketJson), bufferSize);

			Exception error = null;
			ArchipelagoPacketBase receivedPacket = null;
			
			sut.PacketReceived += packet => receivedPacket = packet;
			sut.ErrorReceived += (e, _) => error = e;

			sut.StartPolling();

			int maxRetries = 100;
			int retryCount = 0;
			while (receivedPacket == null && retryCount++ < maxRetries)
				await Task.Delay(10);

			var sayPacket = receivedPacket as SayPacket;

			Assert.That(error, Is.Null);
			Assert.That(sayPacket, Is.Not.Null);
			Assert.That(sayPacket.Text, Is.EqualTo(message));
		}

		[Test]
		public async Task Should_throw_error_failed_parse()
		{
			var faultyJson = @"[{ ""cmd"":""Say"": ""text"": ""Incorrect json"" }]";

			var sut = new BaseArchipelagoSocketHelper<TestWebSocket>(new TestWebSocket(faultyJson), 100);

			Exception error = null;
			ArchipelagoPacketBase receivedPacket = null;

			sut.PacketReceived += packet => receivedPacket = packet;
			sut.ErrorReceived += (e, _) => error = e;

			sut.StartPolling();

			int maxRetries = 100;
			int retryCount = 0;
			while (error == null && retryCount++ < maxRetries)
				await Task.Delay(10);

			Assert.That(receivedPacket, Is.Null);
			Assert.That(error, Is.Not.Null);
			Assert.That(error, Is.TypeOf(typeof(JsonReaderException)));
		}

		[TestCase("some message", 100, Description = "Buffer bigger than message")]
		[TestCase("some message", 10, Description = "Buffer smaller than message")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊", 10, Description = "UTF8 complex character breaking A")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊", 11, Description = "UTF8 complex character breaking B")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊", 12, Description = "UTF8 complex character breaking C")]
		[TestCase("🃁🃂🃃🃄🃅🃆🃇🃈🃉🃊", 13, Description = "UTF8 complex character breaking D")]
		public async Task Should_serialize_and_send_message_to_websocket(string message, int bufferSize)
		{
			var sayPacket = new SayPacket { Text = message };
			
			var testSocket = new TestWebSocket();

			var sut = new BaseArchipelagoSocketHelper<TestWebSocket>(testSocket, bufferSize);

			ArchipelagoPacketBase[] sendPackets = null;

			sut.PacketsSent += packet => sendPackets = packet;

			sut.StartPolling();

			await sut.SendPacketAsync(sayPacket);
			
			int maxRetries = 100;
			int retryCount = 0;
			while (sendPackets == null && retryCount++ < maxRetries)
				await Task.Delay(10);

			Assert.That(sendPackets, Is.Not.Null);
			Assert.That(sendPackets[0], Is.EqualTo(sayPacket));

			var sendMessages = testSocket.GetWrittenMessages();

			Assert.That(sendMessages, Is.Not.Null);

			var json = @"[{""cmd"":""Say"",""text"":""$MESSAGE""}]".Replace("$MESSAGE", message);
			Assert.That(sendMessages[0], Is.EqualTo(json));
		}
	}

	class TestWebSocket : WebSocket
	{
		MemoryStream incommingBytes;

		int currentOutIndex;
		readonly List<MemoryStream> outBytes = new List<MemoryStream> { new MemoryStream() };

		public List<string> GetWrittenMessages() =>
			outBytes.Select(bytes => Encoding.UTF8.GetString(bytes.ToArray())).ToList();

		public TestWebSocket() : this(string.Empty)
		{
		}

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
			// ReSharper disable once AssignNullToNotNullAttribute
			var readCount = await incommingBytes.ReadAsync(outBuffer.Array, 0, outBuffer.Count, cancellationToken);

			return new WebSocketReceiveResult(readCount, WebSocketMessageType.Text, incommingBytes.Position == incommingBytes.Length);
		}

		public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
			CancellationToken cancellationToken)
		{
			if (messageType != WebSocketMessageType.Text)
				return;

			outBytes[currentOutIndex].Write(buffer.Array, buffer.Offset, buffer.Count);

			if (endOfMessage)
			{
				outBytes.Add(new MemoryStream());
				currentOutIndex++;
			}

			await Task.CompletedTask;
		}

		public override WebSocketCloseStatus? CloseStatus => null;
		public override string CloseStatusDescription => null;
		public override string SubProtocol => null;
		public override WebSocketState State => WebSocketState.Open;
	}
}
#endif
