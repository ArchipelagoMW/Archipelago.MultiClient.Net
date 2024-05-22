using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class ArchipelagoSessionFixture
	{
		[Test]
		public void Should_correctly_login()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

			var session = CreateTestSession(socket, fileSystemDataPackageProvider);

			var connectedPacket = new ConnectedPacket
			{
				LocationsChecked = Array.Empty<long>(),
				MissingChecks = Array.Empty<long>(),
				Players = Array.Empty<NetworkPlayer>(),
				SlotData = new Dictionary<string, object>(0),
				SlotInfo = new Dictionary<int, NetworkSlot>(0)
			};

			SetupRoomInfoPacket(socket, new RoomInfoPacket { Games = Array.Empty<string>() });
			SetupLoginResultPacket(socket, connectedPacket);

			var	result = session.TryConnectAndLogin("", "", ItemsHandlingFlags.NoItems);
			
			Assert.That(result.Successful, Is.True);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_request_slot_data_if_enabled(bool requestSlotData)
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

			var session = CreateTestSession(socket, fileSystemDataPackageProvider);

			SetupRoomInfoPacket(socket, new RoomInfoPacket { Games = Array.Empty<string>() });

			_ = session.TryConnectAndLogin("", "", ItemsHandlingFlags.NoItems, requestSlotData: requestSlotData);

			session.Socket.Received().SendPacket(Arg.Is<ConnectPacket>(p => p.RequestSlotData == requestSlotData));
		}

		[Test]
		public void Should_send_connect_after_retrieving_data_package()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

			var session = CreateTestSession(socket, fileSystemDataPackageProvider);

			SetupRoomInfoPacket(socket, new RoomInfoPacket { Games = new []{ "Game1" } });
			SetupLoginResultPacket(socket, new ConnectionRefusedPacket());

			var result = session.TryConnectAndLogin("", "", ItemsHandlingFlags.NoItems);

			Received.InOrder(() =>
			{
				socket.Received().SendPacket(Arg.Any<GetDataPackagePacket>());
				socket.Received().SendPacket(Arg.Any<ConnectPacket>());
			});

			Assert.That(result.Successful, Is.False);
		}

		[Test]
		public void Should_correctly_timeout_when_server_does_not_respond_with_the_roominfo()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

			var session = CreateTestSession(socket, fileSystemDataPackageProvider);

			var result = session.TryConnectAndLogin("", "", ItemsHandlingFlags.NoItems);

			Assert.That(result.Successful, Is.False);
			Assert.That(((LoginFailure)result).Errors.First(), Is.EqualTo("Connection timed out."));
		}

		[Test]
		public void Should_correctly_timeout_when_server_does_not_respond_with_connection_result()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

			var session = CreateTestSession(socket, fileSystemDataPackageProvider);

			SetupRoomInfoPacket(socket, new RoomInfoPacket { Games = Array.Empty<string>() });

			var result = session.TryConnectAndLogin("", "", ItemsHandlingFlags.NoItems);

			socket.Received().SendPacket(Arg.Any<ConnectPacket>());

			Assert.That(result.Successful, Is.False);
			Assert.That(((LoginFailure)result).Errors.First(), Is.EqualTo("Connection timed out."));
		}

		[Test]
		public void Should_add_correct_properties_to_login_successful()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

			var session = CreateTestSession(socket, fileSystemDataPackageProvider);

			var connectedPacket = new ConnectedPacket
			{
				Slot = 4,
				Team = 3,
				LocationsChecked = Array.Empty<long>(),
				MissingChecks = Array.Empty<long>(),
				Players = Array.Empty<NetworkPlayer>(),
				SlotData = new Dictionary<string, object>(0)
			};

			SetupRoomInfoPacket(socket, new RoomInfoPacket { Games = Array.Empty<string>() });
			SetupLoginResultPacket(socket, connectedPacket);

			var result = session.TryConnectAndLogin("", "", ItemsHandlingFlags.NoItems);

			Assert.That(result.Successful, Is.True);

			var successful = (LoginSuccessful)result;

			Assert.That(successful.Slot, Is.EqualTo(4));
			Assert.That(successful.Team, Is.EqualTo(3));
		}

		static ArchipelagoSession CreateTestSession(IArchipelagoSocketHelper socket,
			IFileSystemDataPackageProvider fileSystemDataPackageProvider)
		{
			var dataPackageCache = new DataPackageCache(socket, fileSystemDataPackageProvider);
			var connectionInfo = new ConnectionInfoHelper(socket);
			var locations = new LocationCheckHelper(socket, dataPackageCache, connectionInfo);
			var items = new ReceivedItemsHelper(socket, locations, dataPackageCache, connectionInfo);
			var players = new PlayerHelper(socket, connectionInfo);
			var roomState = new RoomStateHelper(socket, locations);
			var dataStorage = new DataStorageHelper(socket, connectionInfo);
			var messageLog = new MessageLogHelper(socket, items, locations, players, connectionInfo);

			return new ArchipelagoSession(socket, items, locations, players, roomState, connectionInfo, dataStorage, messageLog);
		}

		static void SetupLoginResultPacket(IArchipelagoSocketHelper socket, ArchipelagoPacketBase loginResultPacket) =>
			socket.When(s => s.SendPacket(Arg.Any<ConnectPacket>()))
				.Do(_ => socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(loginResultPacket));

		static void SetupRoomInfoPacket(IArchipelagoSocketHelper socket, RoomInfoPacket roomInfoPacket)
		{
			void SendRoomInfoPacketAsync()
			{
				Task.Factory.StartNew(() =>
				{
					Thread.Sleep(1);

					socket.PacketReceived +=
						Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfoPacket);
				});
			}

#if NET471
			socket.When(s => s.Connect()).Do(_ => SendRoomInfoPacketAsync());
#else
			socket.When(s => s.ConnectAsync()).Do(_ => SendRoomInfoPacketAsync());
#endif
		}
	}
}
