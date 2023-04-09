using Archipelago.MultiClient.Net.Helpers;
using NUnit.Framework;
using System;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class SocketFixture
	{
		[Ignore("for internal use only")]
		[Test]
		public void Should_test_wss_vs_ws()
		{
			var socket = new ArchipelagoSocketHelper(new Uri("ws://archipelago.gg:24242"));

			void Socket_SocketClosed(string reason)
			{
			}

			void Socket_ErrorReceived(Exception e, string message)
			{
			}

			void Socket_SocketOpened()
			{
			}

			void Socket_PacketReceived(ArchipelagoPacketBase packet)
			{
			}

			socket.SocketClosed += Socket_SocketClosed;
			socket.ErrorReceived += Socket_ErrorReceived;
			socket.SocketOpened += Socket_SocketOpened;
			socket.PacketReceived += Socket_PacketReceived;

			try
			{
				socket.Connect();
			}
			catch (Exception e)
			{

			}

			var x = 19;
		}
	}
}
