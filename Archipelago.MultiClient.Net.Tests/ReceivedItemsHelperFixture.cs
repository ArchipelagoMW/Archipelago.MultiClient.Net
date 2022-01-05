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
        public void Enumeratrion_over_collection_should_not_throw_when_new_data_is_received()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locationHelper = Substitute.For<ILocationCheckHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new ReceivedItemsHelper(socket, locationHelper, cache);

            socket.PacketReceived +=
                Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(
                    new ReceivedItemsPacket {
                        Index = 0, 
                        Items = new List<NetworkItem> {
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
                            Items = new List<NetworkItem>{ new NetworkItem { Item = 4 } }
                        });
            });

            Assert.DoesNotThrow(() =>
            {
                enumerateTask.Start();
                receiveNewItemTask.Start();

                Task.WaitAll(enumerateTask, receiveNewItemTask);
            });
        }
    }
}
