using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System.Drawing;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class MessageLogHelperFixture
    {
        [Test]
        public void Should_convert_print_packet_into_string()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            var players = Substitute.For<IPlayerHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            string toStringResult = null;

            sut.OnMessageReceived += (message) =>
            {
                toStringResult = message.ToString();
            };

            var printPacket = new PrintPacket {
                Text = "Some message that really does not add value to the test at hand"
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(printPacket);
            
            Assert.That(toStringResult, Is.EqualTo("Some message that really does not add value to the test at hand"));
        }

        [Test]
        public void Should_convert_print_json_packet_into_string()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            locations.GetLocationNameFromId(6L).Returns("Text6");
            var items = Substitute.For<IReceivedItemsHelper>();
            items.GetItemName(4L).Returns("Text4");
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(8).Returns("Text8");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            string toStringResult = null;

            sut.OnMessageReceived += (message) =>
            {
                toStringResult = message.ToString();
            };

            var packet = new PrintJsonPacket
            {
                Data = new[] {
                    new JsonMessagePart { Type = null, Text = "Text1" },
                    new JsonMessagePart { Type = JsonMessagePartType.Text, Text = "Text2" },
                    new JsonMessagePart { Type = JsonMessagePartType.Color, Text = "Text3", Color = JsonMessagePartColor.Blue },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemId, Text = "4", Flags = ItemFlags.None },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemName, Text = "Text5" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationId, Text = "6" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationName, Text = "Text7" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerId, Text = "8" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerName, Text = "Text9" },
                    new JsonMessagePart { Type = JsonMessagePartType.EntranceName, Text = "Text10" }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);

            Assert.That(toStringResult, Is.EqualTo("Text1Text2Text3Text4Text5Text6Text7Text8Text9Text10"));
        }

        [Test]
        public void Should_get_parsed_data_for_print_packet()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            var players = Substitute.For<IPlayerHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            MessagePart[] parts = null;

            sut.OnMessageReceived += (message) =>
            {
                parts = message.Parts;
            };

            var printPacket = new PrintPacket
            {
                Text = "Some message that really does not add value to the test at hand"
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(printPacket);

            Assert.That(parts.Length, Is.EqualTo(1));
            Assert.That(parts[0].Text, Is.EqualTo("Some message that really does not add value to the test at hand"));
            Assert.That(parts[0].Color, Is.EqualTo(Color.White));
            Assert.That(parts[0].IsBackgroundColor, Is.EqualTo(false));
        }

        [Test]
        public void Should_get_parsed_data_for_print_json_packet()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            locations.GetLocationNameFromId(6L).Returns("Text6");
            var items = Substitute.For<IReceivedItemsHelper>();
            items.GetItemName(4L).Returns("Text4");
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(8).Returns("Text8");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Slot.Returns(0);

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            MessagePart[] parts = null;

            sut.OnMessageReceived += (message) =>
            {
                parts = message.Parts;
            };

            var packet = new PrintJsonPacket
            {
                Data = new[] {
                    new JsonMessagePart { Type = null, Text = "Text1" },
                    new JsonMessagePart { Type = JsonMessagePartType.Text, Text = "Text2" },
                    new JsonMessagePart { Type = JsonMessagePartType.Color, Text = "Text3", Color = JsonMessagePartColor.BlueBg },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemId, Text = "4", Flags = ItemFlags.None },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemName, Text = "Text5" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationId, Text = "6" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationName, Text = "Text7" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerId, Text = "8" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerName, Text = "Text9" },
                    new JsonMessagePart { Type = JsonMessagePartType.EntranceName, Text = "Text10" }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);

            Assert.That(parts.Length, Is.EqualTo(10));
            Assert.That(parts[0].Text, Is.EqualTo("Text1"));
            Assert.That(parts[0].Color, Is.EqualTo(Color.White));
            Assert.That(parts[0].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[1].Text, Is.EqualTo("Text2"));
            Assert.That(parts[1].Color, Is.EqualTo(Color.White));
            Assert.That(parts[1].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[2].Text, Is.EqualTo("Text3"));
            Assert.That(parts[2].Color, Is.EqualTo(Color.Blue));
            Assert.That(parts[2].IsBackgroundColor, Is.EqualTo(true));

            Assert.That(parts[3].Text, Is.EqualTo("Text4"));
            Assert.That(parts[3].Color, Is.EqualTo(Color.Cyan));
            Assert.That(parts[3].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[4].Text, Is.EqualTo("Text5"));
            Assert.That(parts[4].Color, Is.EqualTo(Color.Cyan));
            Assert.That(parts[4].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[5].Text, Is.EqualTo("Text6"));
            Assert.That(parts[5].Color, Is.EqualTo(Color.Green));
            Assert.That(parts[5].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[6].Text, Is.EqualTo("Text7"));
            Assert.That(parts[6].Color, Is.EqualTo(Color.Green));
            Assert.That(parts[6].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[7].Text, Is.EqualTo("Text8"));
            Assert.That(parts[7].Color, Is.EqualTo(Color.Yellow));
            Assert.That(parts[7].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[8].Text, Is.EqualTo("Text9"));
            Assert.That(parts[8].Color, Is.EqualTo(Color.Yellow));
            Assert.That(parts[8].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[9].Text, Is.EqualTo("Text10"));
            Assert.That(parts[9].Color, Is.EqualTo(Color.Blue));
            Assert.That(parts[9].IsBackgroundColor, Is.EqualTo(false));
        }

        [Test]
        public void Should_mark_local_player_as_magenta()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(4).Returns("LocalPlayer");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Slot.Returns(4);

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            MessagePart[] parts = null;

            sut.OnMessageReceived += (message) =>
            {
                parts = message.Parts;
            };

            var packet = new PrintJsonPacket
            {
                Data = new[] {
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerId, Text = "4" }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);

            Assert.That(parts[0].Text, Is.EqualTo("LocalPlayer"));
            Assert.That(parts[0].Color, Is.EqualTo(Color.Magenta));
            Assert.That(parts[0].IsBackgroundColor, Is.EqualTo(false));
        }

        public static object[] ItemColorTestCases = {
            new object[] { ItemFlags.Advancement, Color.Plum },
            new object[] { ItemFlags.Advancement | ItemFlags.NeverExclude, Color.Plum },
            new object[] { ItemFlags.Advancement | ItemFlags.Trap, Color.Plum },
            new object[] { ItemFlags.NeverExclude, Color.SlateBlue },
            new object[] { ItemFlags.NeverExclude | ItemFlags.Trap, Color.SlateBlue },
            new object[] { ItemFlags.Trap, Color.Salmon },
            new object[] { ItemFlags.None, Color.Cyan }
        };

        [TestCaseSource(nameof(ItemColorTestCases))]
        public void Should_mark_progression_items_as_the_correct_color(ItemFlags itemFlags, Color expectedColor)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            items.GetItemName(1L).Returns("ItemFour");
            var players = Substitute.For<IPlayerHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            MessagePart[] parts = null;

            sut.OnMessageReceived += (message) =>
            {
                parts = message.Parts;
            };

            var packet = new PrintJsonPacket
            {
                Data = new[] {
                    new JsonMessagePart { Type = JsonMessagePartType.ItemId, Text = "1", Flags = itemFlags },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemName, Text = "ItemFive", Flags = itemFlags }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);

            Assert.That(parts[0].Text, Is.EqualTo("ItemFour"));
            Assert.That(parts[0].Color, Is.EqualTo(expectedColor));
            Assert.That(parts[0].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[1].Text, Is.EqualTo("ItemFive"));
            Assert.That(parts[1].Color, Is.EqualTo(expectedColor));
            Assert.That(parts[1].IsBackgroundColor, Is.EqualTo(false));
        }
    }
}
