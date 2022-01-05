using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Helpers;
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
    class LocationCheckHelperFixture
    {
        [Test]
        public void Should_not_throw_when_null_or_empty_list_is_checked()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            ILocationCheckHelper sut = new LocationCheckHelper(socket, cache);

            Assert.DoesNotThrow(() => {
                sut.CompleteLocationChecks(null);
                sut.CompleteLocationChecks(Array.Empty<int>());
                sut.CompleteLocationChecksAsync(b => { }, null);
                sut.CompleteLocationChecksAsync(b => { }, Array.Empty<int>());
            });
        }

        [Test]
        public void Should_load_initial_checked_locations_send_by_server()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new LocationCheckHelper(socket, cache);

            var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new List<int>{ 1, 3 },
                MissingChecks = new List<int> { 2 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);
            Assert.Contains(3, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked.ToArray());
            Assert.Contains(3, sut.AllLocationsChecked.ToArray());

            Assert.Contains(2, sut.AllMissingLocations);
        }

        [Test]
        public void Should_add_locations_checked_by_client()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new LocationCheckHelper(socket, cache);

            var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new List<int> { 1 },
                MissingChecks = new List<int> { 2, 3 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);

            sut.CompleteLocationChecks(2);
            sut.CompleteLocationChecksAsync(b => { }, 3);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);
            Assert.Contains(3, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked.ToArray());
            Assert.Contains(2, sut.AllLocationsChecked.ToArray());
            Assert.Contains(3, sut.AllLocationsChecked.ToArray());

            Assert.That(sut.AllMissingLocations, Is.Empty);
        }

        [Test]
        public void Should_add_locations_that_do_not_exists()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new LocationCheckHelper(socket, cache);

            var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new List<int>(),
                MissingChecks = new List<int>()
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);

            sut.CompleteLocationChecks(1);
            sut.CompleteLocationChecksAsync(b => { }, 2);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked.ToArray());
            Assert.Contains(2, sut.AllLocationsChecked.ToArray());

            Assert.That(sut.AllMissingLocations, Is.Empty);
        }

        [Test]
        public void Should_check_locations_send_by_the_server()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new LocationCheckHelper(socket, cache);

            var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new List<int>(),
                MissingChecks = new List<int>{ 1, 2, 3 }
            };

            var roomUpdatePacket = new RoomUpdatePacket
            {
                CheckedLocations = new List<int> { 1, 3 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(roomUpdatePacket);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);
            Assert.Contains(3, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked.ToArray());
            Assert.Contains(3, sut.AllLocationsChecked.ToArray());

            Assert.Contains(2, sut.AllMissingLocations);
        }

        [Test]
        public void Enumeratrion_over_collection_should_not_throw_when_new_data_is_received()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new LocationCheckHelper(socket, cache);

            var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new List<int> { 1, 2, 3 },
                MissingChecks = new List<int>{ 4 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);

            var enumerateTask = new Task(() =>
            {
                var total = 0;

                foreach (var locationId in sut.AllLocationsChecked)
                {
                    Thread.Sleep(1);
                    total += locationId;
                }
            });
            var receiveNewItemTask = new Task(() =>
            {
                Thread.Sleep(1);
                socket.PacketReceived +=
                    Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(
                        new RoomUpdatePacket()
                        {
                            CheckedLocations = new List<int>{ 4 }
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
        public void Should_call_event_handler_when_new_locations_are_checked_by_client()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new LocationCheckHelper(socket, cache);

            var invocationCount = 0;
            var newCheckedLocations = new List<int>();

            sut.CheckedLocationsUpdated += l =>
            {
                invocationCount++;
                newCheckedLocations.AddRange(l);
            };

            sut.CompleteLocationChecks(1, 2);
            sut.CompleteLocationChecksAsync(b => { }, 3);

            Assert.That(invocationCount, Is.EqualTo(2));

            Assert.Contains(1, newCheckedLocations);
            Assert.Contains(2, newCheckedLocations);
            Assert.Contains(3, newCheckedLocations);
        }

        [Test]
        public void Should_call_event_handler_when_new_locations_are_checked_by_server()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var cache = Substitute.For<IDataPackageCache>();

            var sut = new LocationCheckHelper(socket, cache);

            var invocationCount = 0;
            var newCheckedLocations = new List<int>();

            sut.CheckedLocationsUpdated += l =>
            {
                invocationCount++;
                newCheckedLocations.AddRange(l);
            };

            var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new List<int>(),
                MissingChecks = new List<int> { 1, 2, 3 }
            };

            var roomUpdatePacket = new RoomUpdatePacket
            {
                CheckedLocations = new List<int> { 2 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(roomUpdatePacket);

            Assert.That(invocationCount, Is.EqualTo(1));

            Assert.Contains(2, newCheckedLocations);
        }
    }
}
