using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class ReceivedItemsHelperFixture
    {
        [Test]
        public void Enumeration_over_collection_should_not_throw_when_new_data_is_received()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            var players = Substitute.For<IPlayerHelper>();

			var sut = new ReceivedItemsHelper(socket, locationHelper, cache, connectionInfo, players);

            socket.PacketReceived +=
                Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(
                    new ReceivedItemsPacket {
                        Index = 0, 
                        Items = new [] {
                            new NetworkItem { Item = 1 },
                            new NetworkItem { Item = 2 },
                            new NetworkItem { Item = 3 }
                        }
                    });

            var enumerateTask = new Task(() =>
            {
	            // ReSharper disable once NotAccessedVariable
	            var total = 0L;

                foreach (var networkItem in sut.AllItemsReceived)
                {
                    Thread.Sleep(1);
                    total += networkItem.ItemId;
                }
            });
            var receiveNewItemTask = new Task(() =>
            {
                Thread.Sleep(1);
                socket.PacketReceived +=
                    Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(
                        new ReceivedItemsPacket
                        {
                            Index = 3,
                            Items = new []{ new NetworkItem { Item = 4 } }
                        });
            });

            Assert.DoesNotThrow(() =>
            {
                enumerateTask.Start();
                receiveNewItemTask.Start();

                Task.WaitAll(enumerateTask, receiveNewItemTask);
            });
        }

        [Test]
        public void Get_Item_Name_From_DataPackage_Does_Not_Throw()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();
            var players = Substitute.For<IPlayerHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Game.Returns("TestGame");

			var gameDataLookup = Substitute.For<IGameDataLookup>();
			gameDataLookup.Items.Returns(new TwoWayLookup<long, string> { {1, "TestItem"} });
			cache.TryGetGameDataFromCache("TestGame", out Arg.Any<IGameDataLookup>()).Returns(x => 
            {
                x[1] = gameDataLookup;
                return true;
            });

            var sut = new ReceivedItemsHelper(socket, locationHelper, cache, connectionInfo, players);

            sut.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                var itemName = helper.GetItemName(item.ItemId);
                Assert.That(itemName, Is.EqualTo("TestItem"));
            };

            socket.PacketReceived +=
            Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(
                new ReceivedItemsPacket
                {
                    Index = 0,
                    Items = new[] {
                        new NetworkItem { Item = 1 }
                    }
                });
        }

        [Test]
        public void Get_Item_Name_Returns_Null_For_when_ItemId_Doesnt_Exist()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            var players = Substitute.For<IPlayerHelper>();

			var gameDataLookup = Substitute.For<IGameDataLookup>();
            gameDataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1, "TestItem" } });
            cache.TryGetGameDataFromCache("TestGame", out Arg.Any<IGameDataLookup>()).Returns(x =>
            {
	            x[1] = gameDataLookup;
	            return true;
            });

			var sut = new ReceivedItemsHelper(socket, locationHelper, cache, connectionInfo, players);

            sut.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                var itemName = helper.GetItemName(item.ItemId);
                Assert.That(itemName, Is.Null);
            };

            socket.PacketReceived +=
            Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(
                new ReceivedItemsPacket
                {
                    Index = 0,
                    Items = new[] {
                        new NetworkItem { Item = 2 }
                    }
                });
        }

        //https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/issues/46
        [Test]
        public void Receiving_Same_Item_From_Same_Location_Should_Add_To_Queue()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            var players = Substitute.For<IPlayerHelper>();

			var sut = new ReceivedItemsHelper(socket, locationHelper, cache, connectionInfo, players);
            var itemInPacket = new NetworkItem { Item = 1, Location = 1, Player = 1, Flags = Enums.ItemFlags.None };

            sut.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                Assert.That(item, Is.EqualTo(itemInPacket));
            };

            socket.PacketReceived +=
            Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(
                new ReceivedItemsPacket
                {
                    Index = 0,
                    Items = new[] {
                        itemInPacket
                    }
                });

            socket.PacketReceived +=
            Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(
                new ReceivedItemsPacket
                {
                    Index = 1,
                    Items = new[] {
                        itemInPacket
                    }
                });

            Assert.That(sut.Index, Is.EqualTo(2));
        }

		[Test]
		public void Retrieving_item_name_from_id_use_current_game_as_base()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            var players = Substitute.For<IPlayerHelper>();

			var sut = new ReceivedItemsHelper(socket, locationHelper, cache, connectionInfo, players);

            var archipelagoDataLookup = Substitute.For<IGameDataLookup>();
            archipelagoDataLookup.Items.Returns(new TwoWayLookup<long, string> { { -100, "ArchipelagoItem" } });
			var game1DataLookup = Substitute.For<IGameDataLookup>();
            game1DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game1Item" } });
            var game2DataLookup = Substitute.For<IGameDataLookup>();
            game2DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game2Item" } });

            cache.TryGetGameDataFromCache("Archipelago", out Arg.Any<IGameDataLookup>()).Returns(x =>
            {
	            x[1] = archipelagoDataLookup;
	            return true;
            });
			cache.TryGetGameDataFromCache("Game1", out Arg.Any<IGameDataLookup>()).Returns(x =>
            {
	            x[1] = game1DataLookup;
	            return true;
            });
			cache.TryGetGameDataFromCache("Game2", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = game2DataLookup;
				return true;
			});

			connectionInfo.Game.Returns("Game2");
            var itemName = sut.GetItemName(-100);

			Assert.That(itemName, Is.EqualTo("ArchipelagoItem"));
		}
		
		[Test]
		public void Retrieving_item_name_from_id_use_specific_game()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var locationHelper = Substitute.For<ILocationCheckHelper>();
			var cache = Substitute.For<IDataPackageCache>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			var players = Substitute.For<IPlayerHelper>();

			var sut = new ReceivedItemsHelper(socket, locationHelper, cache, connectionInfo, players);

			var archipelagoDataLookup = Substitute.For<IGameDataLookup>();
			archipelagoDataLookup.Items.Returns(new TwoWayLookup<long, string> { { -100, "ArchipelagoItem" } });
			var game1DataLookup = Substitute.For<IGameDataLookup>();
			game1DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game1Item" } });
			var game2DataLookup = Substitute.For<IGameDataLookup>();
			game2DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game2Item" } });

			cache.TryGetGameDataFromCache("Archipelago", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = archipelagoDataLookup;
				return true;
			});
			cache.TryGetGameDataFromCache("Game1", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = game1DataLookup;
				return true;
			});
			cache.TryGetGameDataFromCache("Game2", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = game2DataLookup;
				return true;
			});

			connectionInfo.Game.Returns("Game2");
			var itemName = sut.GetItemName(-100, "Game1");

			Assert.That(itemName, Is.EqualTo("ArchipelagoItem"));
		}
	}
}
