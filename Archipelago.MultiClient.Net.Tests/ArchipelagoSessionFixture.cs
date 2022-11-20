using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class ArchipelagoSessionFixture
	{
		[Test]
		public void Should_send_connect_after_retrieving_data_package()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

			var dataPackageCache = new DataPackageFileSystemCache(socket, fileSystemDataPackageProvider);
			var locations = new LocationCheckHelper(socket, dataPackageCache);
			var items = new ReceivedItemsHelper(socket, locations, dataPackageCache);
			var players = new PlayerHelper(socket);
			var roomState = new RoomStateHelper(socket);
			var connectionInfo = new ConnectionInfoHelper(socket);
			var dataStorage = new DataStorageHelper(socket, connectionInfo);
			var messageLog = new MessageLogHelper(socket, items, locations, players, connectionInfo);

			var session = new ArchipelagoSession(socket, items, locations, players, roomState, connectionInfo, dataStorage, messageLog);


			void SendRoomInfoPacketAsync()
			{
				Task.Factory.StartNew(() =>
				{
					Thread.Sleep(1);

					var roomInfoPacket = new RoomInfoPacket { Games = Array.Empty<string>() };

					socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfoPacket);
				});
			}

#if NET471
			socket.When(s => s.Connect()).Do(_ => SendRoomInfoPacketAsync());
#else
			socket.When(s => s.ConnectAsync()).Do(_ => SendRoomInfoPacketAsync());
#endif
			socket.When(s => s.SendPacket(Arg.Any<ConnectPacket>())).Do(_ =>
			{
				var connectionRefusedPacket = new ConnectionRefusedPacket();

				socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectionRefusedPacket);
			});

			var result = session.TryConnectAndLogin("", "", ItemsHandlingFlags.NoItems);

			Received.InOrder(() => {
				socket.Received().SendPacket(Arg.Any<GetDataPackagePacket>());
				socket.Received().SendPacket(Arg.Any<ConnectPacket>());
			});
			
			Assert.That(result.Successful, Is.False);
		}

		//TODO: make sure TryConnectAndLogin doesnt hang if server fails to send RoomInfoPacket or ConnectionResult
	}
}
