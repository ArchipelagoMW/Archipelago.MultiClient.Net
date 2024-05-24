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
            var cache = Substitute.For<IItemInfoResolver>();
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

        //https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/issues/46
        [Test]
        public void Receiving_Same_Item_From_Same_Location_Should_Add_To_Queue()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var locationHelper = Substitute.For<ILocationCheckHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        var sut = new ReceivedItemsHelper(socket, locationHelper, itemInfoResolver, connectionInfo, players);
	        var itemInPacket = new NetworkItem { Item = 1, Location = 1, Player = 1, Flags = Enums.ItemFlags.None };

	        sut.ItemReceived += (helper) =>
	        {
		        var item = helper.DequeueItem();
		        Assert.That(item.ItemId, Is.EqualTo(itemInPacket.Item));
		        Assert.That(item.LocationId, Is.EqualTo(itemInPacket.Location));
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
	}
}
