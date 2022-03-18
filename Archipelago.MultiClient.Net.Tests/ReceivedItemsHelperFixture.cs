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
                Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(
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
                var total = 0;

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
                    Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(
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
            Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(
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
            Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(
                new ReceivedItemsPacket
                {
                    Index = 0,
                    Items = new[] {
                        new NetworkItem { Item = 2 }
                    }
                });
        }
    }
}
