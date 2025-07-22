using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			var players = Substitute.For<IPlayerHelper>();

            ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

            Assert.DoesNotThrow(() => {
                sut.CompleteLocationChecks(null);
                sut.CompleteLocationChecks(Array.Empty<long>());
#if NET471
                sut.CompleteLocationChecksAsync(b => { }, null);
                sut.CompleteLocationChecksAsync(b => { }, Array.Empty<long>());
#else
                sut.CompleteLocationChecksAsync(null).Wait();
                sut.CompleteLocationChecksAsync(Array.Empty<long>()).Wait();
#endif
            });
        }

        [Test]
        public void Should_load_initial_checked_locations_send_by_server()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new long[]{ 1, 3 },
                MissingChecks = new long[]{ 2 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);
            Assert.Contains(3, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked);
            Assert.Contains(3, sut.AllLocationsChecked);

            Assert.Contains(2, sut.AllMissingLocations);
        }

        [Test]
        public void Should_also_load_initial_missing_locations_send_by_server_if_no_location_has_been_checked_yet()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = Array.Empty<long>(),
                MissingChecks = new long[] { 1, 2 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);

            Assert.IsEmpty(sut.AllLocationsChecked);

            Assert.Contains(1, sut.AllMissingLocations);
            Assert.Contains(2, sut.AllMissingLocations);
        }

        [Test]
        public void Should_also_load_initial_checked_locations_send_by_server_if_no_location_is_missing()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new long[] { 1, 2 },
                MissingChecks = Array.Empty<long>()
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked);
            Assert.Contains(2, sut.AllLocationsChecked);

            Assert.IsEmpty(sut.AllMissingLocations);
        }

        [Test]
        public void Should_add_locations_checked_by_client()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new long[]{ 1 },
                MissingChecks = new long[]{ 2, 3 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            sut.CompleteLocationChecks(2);
#if NET471
            sut.CompleteLocationChecksAsync(b => { }, 3);
#else
            sut.CompleteLocationChecksAsync(3).Wait();
#endif

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);
            Assert.Contains(3, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked);
            Assert.Contains(2, sut.AllLocationsChecked);
            Assert.Contains(3, sut.AllLocationsChecked);

            Assert.That(sut.AllMissingLocations, Is.Empty);
        }

		[Test]
        public void Should_check_locations_send_by_the_server()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = Array.Empty<long>(),
                MissingChecks = new long[]{ 1, 2, 3 }
            };

            var roomUpdatePacket = new RoomUpdatePacket
            {
                CheckedLocations = new long[]{ 1, 3 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomUpdatePacket);

            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);
            Assert.Contains(3, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked);
            Assert.Contains(3, sut.AllLocationsChecked);

            Assert.Contains(2, sut.AllMissingLocations);
        }

        [Test]
        public void Enumeration_over_collection_should_not_throw_when_new_data_is_received()
		{
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new long[]{ 1, 2, 3 },
                MissingChecks = new long[]{ 4 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            var enumerateTask = new Task(() =>
            {
                long total = 0;

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
                    Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(
                        new RoomUpdatePacket()
                        {
                            CheckedLocations = new long[]{ 4 }
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
        public void Should_call_event_handler_when_new_locations_are_checked_by_server()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var newCheckedLocations = new List<long[]>();

            sut.CheckedLocationsUpdated += l =>
            {
                newCheckedLocations.Add(l.ToArray());
            };

            var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new long[]{ 3 },
                MissingChecks = new long[]{ 1, 2 }
            };

            var roomUpdatePacket = new RoomUpdatePacket
            {
                CheckedLocations = new long[]{ 2 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomUpdatePacket);

            Assert.That(newCheckedLocations.Count, Is.EqualTo(2));

            Assert.Contains(3, newCheckedLocations[0]);
            Assert.Contains(2, newCheckedLocations[1]);
        }

        [Test]
        public void Should_not_call_event_handler_when_no_new_locations_are_checked()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = new long[]{ 1, 2 },
                MissingChecks = new long[]{ 3 }
            };
            var roomUpdatePacket = new RoomUpdatePacket
            {
                CheckedLocations = new long[]{ 1 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomUpdatePacket);

            var invocationCount = 0;
            sut.CheckedLocationsUpdated += l =>
            {
                invocationCount++;
            };

            sut.CompleteLocationChecks(null);
            sut.CompleteLocationChecks(Array.Empty<long>());
            sut.CompleteLocationChecks(1);
#if NET471
            sut.CompleteLocationChecksAsync(b => { }, null);
            sut.CompleteLocationChecksAsync(b => { }, Array.Empty<long>());
            sut.CompleteLocationChecksAsync(b => { }, 1);
#else
            sut.CompleteLocationChecksAsync(null).Wait();
            sut.CompleteLocationChecksAsync(Array.Empty<long>()).Wait();
            sut.CompleteLocationChecksAsync(1).Wait();
#endif

            Assert.That(invocationCount, Is.Zero);
        }

        [Test]
        public void Should_not_check_duplicated_locations()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
            {
                LocationsChecked = Array.Empty<long>(),
                MissingChecks = new long[]{ 1, 2, 3 }
            };
            var roomUpdatePacket = new RoomUpdatePacket
            {
                CheckedLocations = new long[]{ 2, 3 }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomUpdatePacket);

            sut.CompleteLocationChecks(1, 2);
#if NET471
            sut.CompleteLocationChecksAsync(b => { }, 1, 3);
#else
            sut.CompleteLocationChecksAsync(1, 3).Wait();
#endif
            
            Assert.Contains(1, sut.AllLocations);
            Assert.Contains(2, sut.AllLocations);
            Assert.Contains(3, sut.AllLocations);

            Assert.Contains(1, sut.AllLocationsChecked);
            Assert.Contains(2, sut.AllLocationsChecked);
            Assert.Contains(3, sut.AllLocationsChecked);

            Assert.That(sut.AllMissingLocations, Is.Empty);
        }


        [Test]
        public void Should_not_send_location_checks_already_confirmed_by_the_server()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			var players = Substitute.For<IPlayerHelper>();

			ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
	        {
		        LocationsChecked = new long[] { 1, 2 },
		        MissingChecks = new long[] { 3, 4, 5, 6 },
	        };

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

	        var updatePacket = new RoomUpdatePacket
	        {
		        CheckedLocations = new long[] { 3, 4 }
	        };

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(updatePacket);

	        sut.CompleteLocationChecks(2, 4, 6);

	        socket.Received().SendPacket(Arg.Is<LocationChecksPacket>(p => p.Locations.Length == 1 && p.Locations.First() == 6L));
        }

		[Test]
        public void Should_re_send_location_checks_already_checked_but_not_confirmed_by_server()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			var players = Substitute.For<IPlayerHelper>();

			ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
	        {
		        LocationsChecked = new long[] { 1 },
		        MissingChecks = new long[] { 2, 3, 4, 5, 6 },
			};

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

			sut.CompleteLocationChecks(2, 3);

			socket.Received().SendPacket(Arg.Is<LocationChecksPacket>(p => p.Locations.Length == 2));

			sut.CompleteLocationChecks(4);
			
			socket.Received().SendPacket(Arg.Is<LocationChecksPacket>(p => p.Locations.Length == 3));

			sut.CompleteLocationChecks(5, 6);

			socket.Received().SendPacket(Arg.Is<LocationChecksPacket>(p => p.Locations.Length == 5));
		}

        [Test]
        public void Should_not_send_check_if_no_new_locations_are_checked()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
	        {
		        LocationsChecked = new long[] { 1, 2, 3 },
		        MissingChecks = new long[] { 5, 6 },
	        };

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

	        sut.CompleteLocationChecks(2, 3);

	        socket.DidNotReceive().SendPacket(Arg.Any<LocationChecksPacket>());
        }

        [Test]
        public void Should_not_fail_when_room_update_is_missing_location_checks()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        var players = Substitute.For<IPlayerHelper>();

	        _ = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connectedPacket = new ConnectedPacket
	        {
		        LocationsChecked = Array.Empty<long>(),
		        MissingChecks = new long[] { 1, 2, 3 }
	        };

	        var roomUpdatePacket = new RoomUpdatePacket
	        {
		        CheckedLocations = null
	        };

	        Assert.DoesNotThrow(() =>
	        {
		        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);
		        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomUpdatePacket);
	        });
        }

#if !NET471
		[Test]
        public void Should_scout_locations_async_but_only_existing_ones()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(1);
			connectionInfo.Slot.Returns(2);

			var players = Substitute.For<IPlayerHelper>();
#if NET472
	        players.Players.Returns(
		        new Dictionary<int, ReadOnlyCollection<PlayerInfo>> {
			        { 1, new ReadOnlyCollection<PlayerInfo>(new List<PlayerInfo> { null, null, new PlayerInfo() }) }
		        });
#else
	        players.Players.Returns(new ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>>(
		        new Dictionary<int, ReadOnlyCollection<PlayerInfo>> {
			        { 1, new ReadOnlyCollection<PlayerInfo>(new List<PlayerInfo> { null, null, new PlayerInfo() }) }
				}));
#endif
			ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var connected = new ConnectedPacket
			{
				LocationsChecked = new long[0],
				MissingChecks = new[] { 1L }
			};
			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connected);

			var locationScoutResponse = new LocationInfoPacket()
            {
                Locations = new [] { new NetworkItem { Location = 1 } }
            };

            var scoutTask = sut.ScoutLocationsAsync(1, 2);

            Assert.That(scoutTask.IsCompleted, Is.False);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(locationScoutResponse);

            scoutTask.Wait();

            socket.Received().SendPacketAsync(Arg.Is<LocationScoutsPacket>(p => p.Locations.Length == 1 && p.Locations[0] == 1));

			Assert.That(scoutTask.IsCompleted, Is.True);
			Assert.That(scoutTask.Result, Is.Not.Null);
			Assert.That(scoutTask.Result.Count, Is.EqualTo(1));
			Assert.That(scoutTask.Result.First().Key, Is.EqualTo(1));
		}

		[Test]
		public void Should_scout_locations_should_still_call_callback_if_no_locations_exist()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(1);
			connectionInfo.Slot.Returns(2);

			var players = Substitute.For<IPlayerHelper>();
#if NET472
	        players.Players.Returns(
		        new Dictionary<int, ReadOnlyCollection<PlayerInfo>> {
			        { 1, new ReadOnlyCollection<PlayerInfo>(new List<PlayerInfo> { null, null, new PlayerInfo() }) }
		        });
#else
			players.Players.Returns(new ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>>(
				new Dictionary<int, ReadOnlyCollection<PlayerInfo>> {
					{ 1, new ReadOnlyCollection<PlayerInfo>(new List<PlayerInfo> { null, null, new PlayerInfo() }) }
				}));
#endif
			ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

			var locationScoutResponse = new LocationInfoPacket()
			{
				Locations = new[] { new NetworkItem { Location = 1 } }
			};

			var scoutTask = sut.ScoutLocationsAsync(1, 2);

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(locationScoutResponse);

			scoutTask.Wait();

			socket.DidNotReceive().SendPacketAsync(Arg.Any<LocationScoutsPacket>());

			Assert.That(scoutTask.IsCompleted, Is.True);
			Assert.That(scoutTask.Result, Is.Not.Null);
			Assert.That(scoutTask.Result.Count, Is.EqualTo(0));
		}
#else
		[Test]
        public void Should_scout_locations_async_but_only_existing_ones()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(1);
			connectionInfo.Slot.Returns(2);

			var players = Substitute.For<IPlayerHelper>();

	        players.Players.Returns(
		        new Dictionary<int, ReadOnlyCollection<PlayerInfo>> {
			        { 1, new ReadOnlyCollection<PlayerInfo>(new List<PlayerInfo> { null, null, new PlayerInfo() }) }
		        });

			ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);
			
			var connected = new ConnectedPacket
			{
				LocationsChecked = new long[0], 
				MissingChecks = new[] { 1L }
			};
			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connected);

			Dictionary<long, ScoutedItemInfo> scoutedLocations = null;

            sut.ScoutLocationsAsync(scouted => { scoutedLocations = scouted; }, 1, 2);

            var locationScoutResponse = new LocationInfoPacket
            {
	            Locations = new[] { new NetworkItem { Location = 1 } }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(locationScoutResponse);

			socket.Received().SendPacketAsync(Arg.Is<LocationScoutsPacket>(p => p.Locations.Length == 1 && p.Locations[0] == 1));

			Assert.That(scoutedLocations, Is.Not.Null);
            Assert.That(scoutedLocations.Count, Is.EqualTo(1));
			Assert.That(scoutedLocations.First().Key, Is.EqualTo(1));
		}

        [Test]
        public void Should_scout_locations_should_still_call_callback_if_no_locations_exist()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();
	        connectionInfo.Team.Returns(1);
	        connectionInfo.Slot.Returns(2);

	        var players = Substitute.For<IPlayerHelper>();

	        players.Players.Returns(
		        new Dictionary<int, ReadOnlyCollection<PlayerInfo>> {
			        { 1, new ReadOnlyCollection<PlayerInfo>(new List<PlayerInfo> { null, null, new PlayerInfo() }) }
		        });

	        ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

	        Dictionary<long, ScoutedItemInfo> scoutedLocations = null;

	        sut.ScoutLocationsAsync(scouted => { scoutedLocations = scouted; }, 999);

	        socket.DidNotReceive().SendPacketAsync(Arg.Any<LocationScoutsPacket>());

	        Assert.That(scoutedLocations, Is.Not.Null);
	        Assert.That(scoutedLocations.Count, Is.EqualTo(0));
        }
#endif

		[Test]
	    public void Should_ignore_non_existing_locations()
	    {
		    var socket = Substitute.For<IArchipelagoSocketHelper>();
		    var itemInfoResolver = Substitute.For<IItemInfoResolver>();
		    var connectionInfo = Substitute.For<IConnectionInfoProvider>();
		    var players = Substitute.For<IPlayerHelper>();

		    ILocationCheckHelper sut = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, players);

		    var connectedPacket = new ConnectedPacket
		    {
			    LocationsChecked = new long[] { 1, 2, 3 },
			    MissingChecks = new long[] { 5, 6 },
		    };

		    socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

		    sut.CompleteLocationChecks(5, 7);

		    socket.Received().SendPacket(Arg.Is<LocationChecksPacket>(p => p.Locations.Length == 1 && p.Locations[0] == 5));
	    }
	}
}
