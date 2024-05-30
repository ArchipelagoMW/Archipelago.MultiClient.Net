using Archipelago.MultiClient.Net.Helpers;
using NUnit.Framework;
using System;
using System.Net;

#if NET471
#else
using System.Net.WebSockets;
#endif

namespace Archipelago.MultiClient.Net.Tests
{
	[Ignore("Can only be manually run after spinning up a server")]
	[TestFixture]
	class SocketFixture
	{
		const string WssServer = "archipelago.gg:43929";
		const string WsServer = "localhost:38281";

		[TestCase("ws://" + WsServer)]
		[TestCase("wss://" + WssServer)]
		[TestCase("unspecified://" + WsServer)]
		[TestCase("unspecified://" + WssServer)]
		public void Should_connect_over_ws_or_wss(string server)
		{
			var socketIsOpen = false;
			var errors = "";

			var socket = new ArchipelagoSocketHelper(new Uri(server));

			socket.SocketClosed += (reason) => errors += $"Socket closed: {reason}";
			socket.ErrorReceived += (sender, error) => errors += $"Socket error received: {error}";

			socket.SocketOpened += () => socketIsOpen = true;

#if NET471
			Assert.DoesNotThrow(() =>
			{
				socket.Connect();
			});
#else
			Assert.DoesNotThrowAsync(async () =>
			{
				await socket.ConnectAsync();
			});
#endif

			Assert.IsTrue(socketIsOpen);
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void Should_timeout_on_non_existing_server()
		{
			var socketIsOpen = false;
			var errors = "";
			string closedMsg = null;

			var socket = new ArchipelagoSocketHelper(new Uri("wss://notaurl.gg:10000"));

			socket.SocketClosed += (reason) => closedMsg = reason;
			socket.ErrorReceived += (sender, error) => errors += $"Socket error received: {error}";

			socket.SocketOpened += () => socketIsOpen = true;

#if NET471
			Assert.DoesNotThrow(() =>
			{
				socket.Connect();
			});
			Assert.That(closedMsg, Is.EqualTo("An exception has occurred while attempting to connect."));
#elif NET472
			Assert.DoesNotThrowAsync(async () =>
			{
				await socket.ConnectAsync();
			});
			Assert.That(closedMsg, Is.EqualTo("An exception has occurred while attempting to connect."));
#else
			var e = Assert.ThrowsAsync<WebSocketException>(async () =>
			{
				await socket.ConnectAsync();
			});
			Assert.That(((WebException)e.InnerException).Status, Is.EqualTo(WebExceptionStatus.NameResolutionFailure));
#endif

			Assert.IsFalse(socketIsOpen);

#if NET471 || NET472
			Assert.That(errors, Is.Empty);
#else
			Assert.IsTrue(errors.StartsWith("Socket error received:"));
#endif
		}

		[Test]
		public void Should_timeout_on_invalid_port()
		{
			var socketIsOpen = false;
			var errors = "";
			string closedMsg = null;

			var socket = new ArchipelagoSocketHelper(new Uri("ws://archipelago.gg:1"));

			socket.SocketClosed += (reason) => closedMsg = reason;
			socket.ErrorReceived += (sender, error) => errors += $"Socket error received: {error}";

			socket.SocketOpened += () => socketIsOpen = true;

#if NET471
			Assert.DoesNotThrow(() =>
			{
				socket.Connect();
			});
			Assert.That(closedMsg, Is.EqualTo("An exception has occurred while attempting to connect."));
#elif NET472
			Assert.DoesNotThrowAsync(async () =>
			{
				await socket.ConnectAsync();
			});
			Assert.That(closedMsg, Is.EqualTo("An exception has occurred while attempting to connect."));
#else
			var e = Assert.ThrowsAsync<WebSocketException>(async () =>
			{
				await socket.ConnectAsync();
			});
			Assert.That(((WebException)e.InnerException).Status, Is.EqualTo(WebExceptionStatus.ConnectFailure));
#endif

			Assert.IsFalse(socketIsOpen);

#if NET471 || NET472
			Assert.That(errors, Is.Empty);
#else
			Assert.IsTrue(errors.StartsWith("Socket error received:"));
#endif
		}
	}
}