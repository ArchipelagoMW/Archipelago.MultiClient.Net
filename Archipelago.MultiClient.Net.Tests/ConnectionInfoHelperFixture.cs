using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class ConnectionInfoHelperFixture
    {
        [Test]
        public void Should_update_slot_and_team()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new ConnectionInfoHelper(socket);

            var packet = new ConnectedPacket { Team = 23, Slot = 578 };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);

            Assert.That(sut.Team, Is.EqualTo(23));
            Assert.That(sut.Slot, Is.EqualTo(578));
        }

        [Test]
        public void Should_take_over_provided_connection_parameters()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new ConnectionInfoHelper(socket);

            sut.SetConnectionParameters("SomeGame", new []{ "TAGS" }, ItemsHandlingFlags.IncludeOwnItems, "MyId");

            Assert.That(sut.Game, Is.EqualTo("SomeGame"));
            Assert.That(sut.Tags, Is.EqualTo(new[] { "TAGS" }));
            Assert.That(sut.ItemsHandlingFlags, Is.EqualTo(ItemsHandlingFlags.IncludeOwnItems));
            Assert.That(sut.Uuid, Is.EqualTo("MyId"));
        }

        [Test]
        public void Should_set_initial_values()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new ConnectionInfoHelper(socket);

            Assert.That(sut.Game, Is.Null);
            Assert.That(sut.Team, Is.EqualTo(-1));
            Assert.That(sut.Slot, Is.EqualTo(-1));
            Assert.That(sut.Tags, Is.EqualTo(Array.Empty<string>()));
            Assert.That(sut.ItemsHandlingFlags, Is.EqualTo(ItemsHandlingFlags.NoItems));
            Assert.That(sut.Uuid, Is.Null);
        }

        [Test] 
        public void Should_reset_values_on_connection_failure()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new ConnectionInfoHelper(socket);

            var connectedPacket = new ConnectedPacket { Team = 23, Slot = 578 };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);

            sut.SetConnectionParameters("SomeGame", new[] { "TAGS" }, ItemsHandlingFlags.IncludeOwnItems, "MyId");

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(new ConnectionRefusedPacket());

            Assert.That(sut.Game, Is.Null);
            Assert.That(sut.Team, Is.EqualTo(-1));
            Assert.That(sut.Slot, Is.EqualTo(-1));
            Assert.That(sut.Tags, Is.EqualTo(Array.Empty<string>()));
            Assert.That(sut.ItemsHandlingFlags, Is.EqualTo(ItemsHandlingFlags.NoItems));
            Assert.That(sut.Uuid, Is.Null);
        }
    }
}
