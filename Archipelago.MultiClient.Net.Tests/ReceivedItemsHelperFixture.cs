using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
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

            var sut = new ReceivedItemsHelper(socket, locationHelper, cache);

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
                var total = 0L;

                foreach (var networkItem in sut.AllItemsReceived)
                {
                    Thread.Sleep(1);
                    total += networkItem.Item;
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

            var localCache = new DataPackage()
            {
                Games = new Dictionary<string, GameData>()
                {
                    ["TestGame"] = new GameData()
                    {
                        ItemLookup = new Dictionary<string, long>()
                        {
                            ["TestItem"] = 1
                        }
                    }
                }
            };
            cache.TryGetDataPackageFromCache(out Arg.Any<DataPackage>()).Returns(x => 
            {
                x[0] = localCache;
                return true;
            });

            var sut = new ReceivedItemsHelper(socket, locationHelper, cache);

            sut.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                var itemName = helper.GetItemName(item.Item);
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
        public void Get_Item_Name_Returns_Null_For_Invalid_ItemId_Where_Game_Doesnt_Exist()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var localCache = new DataPackage()
            {
                Games = new Dictionary<string, GameData>()
                {
                    ["TestGame"] = new GameData()
                    {
                        ItemLookup = new Dictionary<string, long>()
                        {
                            ["TestItem"] = 1
                        }
                    }
                }
            };
            cache.TryGetDataPackageFromCache(out Arg.Any<DataPackage>()).Returns(x =>
            {
                x[0] = localCache;
                return true;
            });

            var sut = new ReceivedItemsHelper(socket, locationHelper, cache);

            sut.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                var itemName = helper.GetItemName(item.Item);
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

            var localCache = new DataPackage()
            {
                Games = new Dictionary<string, GameData>()
                {
                    ["TestGame"] = new GameData()
                    {
                        ItemLookup = new Dictionary<string, long>()
                        {
                            ["TestItem"] = 1
                        }
                    }
                }
            };
            cache.TryGetDataPackageFromCache(out Arg.Any<DataPackage>()).Returns(x =>
            {
                x[0] = localCache;
                return true;
            });

            var sut = new ReceivedItemsHelper(socket, locationHelper, cache);
            var itemInPacket = new NetworkItem { Item = 1, Location = 1, Player = 1, Flags = Enums.ItemFlags.None };

            sut.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                var itemName = helper.GetItemName(item.Item);
                Assert.That(item, Is.EqualTo(itemInPacket));
                Assert.That(itemName, Is.EqualTo("TestItem"));
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

		//TODO ADD item name retrieval + duplicated ideez


		[Test]
		public void Retrieving_item_name_from_id_use_current_game_as_base()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new ReceivedItemsHelper(socket, locationHelper, cache, connectionInfo);

            var localCache = new DataPackage
            {
	            Games = new Dictionary<string, GameData>
	            {
		            ["Game1"] = new GameData {
			            ItemLookup = new Dictionary<string, long> {
				            ["Game1Item"] = 1337
			            }
		            },
		            ["Game2"] = new GameData {
			            ItemLookup = new Dictionary<string, long> {
				            ["Game2Item"] = 1337
			            }
		            },
				}
            };

            connectionInfo.Game.Returns("Game2");
			cache.TryGetDataPackageFromCache(out Arg.Any<DataPackage>()).Returns(x =>
            {
	            x[0] = localCache;
	            return true;
            });


            var itenName = sut.GetItemName(1337);

			Assert.That(itenName, Is.EqualTo("Game2Item"));
		}

		//TODO negative item ideez
    }
}
