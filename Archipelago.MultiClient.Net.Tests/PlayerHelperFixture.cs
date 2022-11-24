using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Tests
{
    class PlayerHelperFixture
    {
        [Test]
        public void Should_return_empty_collection_when_not_yet_initialized()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new PlayerHelper(socket);

            Assert.That(sut.AllPlayers, Is.Empty);
        }

        [Test]
        public void Should_add_players_from_connected_packet()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new PlayerHelper(socket);

            var connectedPacket = new ConnectedPacket {
                Players = new[] {
                    new NetworkPlayer { Name = "1", Alias = "One", Slot = 1, Team = 1 },
                    new NetworkPlayer { Name = "2", Alias = "Two", Slot = 2, Team = 1 },
                    new NetworkPlayer { Name = "3", Alias = "Three", Slot = 1, Team = 2 }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            var playerOne = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 1);
            Assert.That(playerOne.Name, Is.EqualTo("1"));
            Assert.That(playerOne.Alias, Is.EqualTo("One"));

            var playerTwo = sut.AllPlayers.First(p => p.Slot == 2 && p.Team == 1);
            Assert.That(playerTwo.Name, Is.EqualTo("2"));
            Assert.That(playerTwo.Alias, Is.EqualTo("Two"));

            var playerThree = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 2);
            Assert.That(playerThree.Name, Is.EqualTo("3"));
            Assert.That(playerThree.Alias, Is.EqualTo("Three"));
        }

        [Test]
        public void Should_add_games_from_connected_packet()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new PlayerHelper(socket);

            var connectedPacket = new ConnectedPacket {
                Players = new[] {
                    new NetworkPlayer { Name = "1", Alias = "One", Slot = 1, Team = 1 },
                    new NetworkPlayer { Name = "2", Alias = "Two", Slot = 2, Team = 1 },
                    new NetworkPlayer { Name = "3", Alias = "Three", Slot = 1, Team = 2 }
                },
                SlotInfo = new Dictionary<int, NetworkSlot> {
                    { 1, new NetworkSlot { Type = SlotType.Player, Game = "Game1" } },
                    { 2, new NetworkSlot { Type = SlotType.Player, Game = "Game2" } }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            var playerOne = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 1);
            Assert.That(playerOne.Game, Is.EqualTo("Game1"));

            var playerTwo = sut.AllPlayers.First(p => p.Slot == 2 && p.Team == 1);
            Assert.That(playerTwo.Game, Is.EqualTo("Game2"));

            var playerThree = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 2);
            Assert.That(playerThree.Game, Is.EqualTo("Game1"));
        }

        [Test]
        public void Should_add_groups_to_players_from_connected_packet()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new PlayerHelper(socket);

            var connectedPacket = new ConnectedPacket
            {
                Players = new[] {
                    new NetworkPlayer { Name = "1", Alias = "One", Slot = 1, Team = 1 },
                    new NetworkPlayer { Name = "2", Alias = "Two", Slot = 2, Team = 1 },
                    new NetworkPlayer { Name = "3", Alias = "Three", Slot = 3, Team = 1 },
                },
                SlotInfo = new Dictionary<int, NetworkSlot> {
                    { 1, new NetworkSlot { Type = SlotType.Player, Game = "Game1" } },
                    { 2, new NetworkSlot { Type = SlotType.Player, Game = "Game2" } },
                    { 3, new NetworkSlot { Type = SlotType.Player, Game = "Game1" } },
                    { 4, new NetworkSlot { Type = SlotType.Group, Game = "Game1",
                        Name = "Player3Personal", GroupMembers = new []{ 3 }} },
                    { 5, new NetworkSlot { Type = SlotType.Group, Game = "Game1", 
                        Name = "Game1All", GroupMembers = new []{ 1,3 }} },
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            var playerOne = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 1);
            Assert.That(playerOne.Groups.Length, Is.EqualTo(1));
            var playerOneGroup = playerOne.Groups.First();
            Assert.That(playerOneGroup.Name, Is.EqualTo("Game1All"));
            Assert.That(playerOneGroup.Game, Is.EqualTo("Game1"));
            Assert.That(playerOneGroup.GroupMembers, Is.EquivalentTo(new []{ 1, 3 }));
            
            var playerTwo = sut.AllPlayers.First(p => p.Slot == 2 && p.Team == 1);
            Assert.That(playerTwo.Groups, Is.Empty);

            var playerThree = sut.AllPlayers.First(p => p.Slot == 3 && p.Team == 1);
            Assert.That(playerThree.Groups.Length, Is.EqualTo(2));
            var playerThreeGroupGame1All = playerThree.Groups.First(g => g.Name == "Game1All");
            Assert.That(playerThreeGroupGame1All.Game, Is.EqualTo("Game1"));
            Assert.That(playerThreeGroupGame1All.GroupMembers, Is.EquivalentTo(new[] { 1, 3 }));
            var playerThreeGroupPersonal = playerThree.Groups.First(g => g.Name == "Player3Personal");
            Assert.That(playerThreeGroupPersonal.Game, Is.EqualTo("Game1"));
            Assert.That(playerThreeGroupPersonal.GroupMembers, Is.EquivalentTo(new[] { 3 }));
        }

        [Test]
        public void Should_update_info_from_room_updated_packet()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new PlayerHelper(socket);

            var connectedPacket = new ConnectedPacket
            {
                Players = new[] {
                    new NetworkPlayer { Name = "1", Alias = "One", Slot = 1, Team = 1 },
                    new NetworkPlayer { Name = "2", Alias = "Two", Slot = 1, Team = 2 },
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            var playerOne = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 1);
            Assert.That(playerOne.Name, Is.EqualTo("1"));
            Assert.That(playerOne.Alias, Is.EqualTo("One"));

            var playerTwo = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 2);
            Assert.That(playerTwo.Name, Is.EqualTo("2"));
            Assert.That(playerTwo.Alias, Is.EqualTo("Two"));

            var roomInfoUpdatedPacket = new RoomUpdatePacket {
                Players = new[] {
                    new NetworkPlayer { Name = "Henk", Alias = "Terminator", Slot = 1, Team = 1 },
                    new NetworkPlayer { Name = "Frank", Alias = "Destroyer", Slot = 2, Team = 1 },
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfoUpdatedPacket);
            
            var playerOneUpdated = sut.AllPlayers.First(p => p.Slot == 1 && p.Team == 1);
            Assert.That(playerOneUpdated.Name, Is.EqualTo("Henk"));
            Assert.That(playerOneUpdated.Alias, Is.EqualTo("Terminator"));

            var playerTwoUpdated = sut.AllPlayers.First(p => p.Slot == 2 && p.Team == 1);
            Assert.That(playerTwoUpdated.Name, Is.EqualTo("Frank"));
            Assert.That(playerTwoUpdated.Alias, Is.EqualTo("Destroyer"));
        }

        [Test]
        public void Should_not_crash_when_room_update_does_not_contain_players()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new PlayerHelper(socket);

            var connectedPacket = new ConnectedPacket
            {
                Players = new[] {
                    new NetworkPlayer { Name = "1", Alias = "One", Slot = 1, Team = 1 },
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(connectedPacket);

            var roomInfoUpdatedPacket = new RoomUpdatePacket();

            Assert.DoesNotThrow(() => {
                socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfoUpdatedPacket);
            });

            Assert.That(sut.AllPlayers.Count, Is.EqualTo(1));
        }
    }
}
