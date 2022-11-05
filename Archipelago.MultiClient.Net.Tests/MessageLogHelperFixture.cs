using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

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

            sut.OnMessageReceived += (message) => toStringResult = message.ToString();

            var printPacket = new PrintPacket {
                Text = "Some message that really does not add value to the test at hand"
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(printPacket);

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

            sut.OnMessageReceived += (message) => toStringResult = message.ToString();

            var packet = new PrintJsonPacket {
                Data = new[] {
                    new JsonMessagePart { Type = null, Text = "Text1" },
                    new JsonMessagePart { Type = JsonMessagePartType.Text, Text = "Text2" },
                    new JsonMessagePart {
                        Type = JsonMessagePartType.Color, Text = "Text3", Color = JsonMessagePartColor.Blue
                    },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemId, Text = "4", Flags = ItemFlags.None },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemName, Text = "Text5" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationId, Text = "6" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationName, Text = "Text7" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerId, Text = "8" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerName, Text = "Text9" },
                    new JsonMessagePart { Type = JsonMessagePartType.EntranceName, Text = "Text10" }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

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

            sut.OnMessageReceived += (message) => parts = message.Parts;

            var printPacket = new PrintPacket { Text = "Some message that really does not add value to the test at hand" };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(printPacket);

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

            sut.OnMessageReceived += (message) => parts = message.Parts;

            var packet = new PrintJsonPacket {
                Data = new[] {
                    new JsonMessagePart { Type = null, Text = "Text1" },
                    new JsonMessagePart { Type = JsonMessagePartType.Text, Text = "Text2" },
                    new JsonMessagePart {
                        Type = JsonMessagePartType.Color, Text = "Text3", Color = JsonMessagePartColor.BlueBg
                    },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemId, Text = "4", Flags = ItemFlags.None },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemName, Text = "Text5" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationId, Text = "6" },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationName, Text = "Text7" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerId, Text = "8" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerName, Text = "Text9" },
                    new JsonMessagePart { Type = JsonMessagePartType.EntranceName, Text = "Text10" }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

            Assert.That(parts.Length, Is.EqualTo(10));
            Assert.That(parts[0].Text, Is.EqualTo("Text1"));
            Assert.That(parts[0].Color, Is.EqualTo(Color.White));
            Assert.That(parts[0].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[0].Type, Is.EqualTo(MessagePartType.Text));

            Assert.That(parts[1].Text, Is.EqualTo("Text2"));
            Assert.That(parts[1].Color, Is.EqualTo(Color.White));
            Assert.That(parts[1].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[1].Type, Is.EqualTo(MessagePartType.Text));

            Assert.That(parts[2].Text, Is.EqualTo("Text3"));
            Assert.That(parts[2].Color, Is.EqualTo(Color.Blue));
            Assert.That(parts[2].IsBackgroundColor, Is.EqualTo(true));
            Assert.That(parts[2].Type, Is.EqualTo(MessagePartType.Text));

            Assert.That(parts[3].Text, Is.EqualTo("Text4"));
            Assert.That(parts[3].Color, Is.EqualTo(Color.Cyan));
            Assert.That(parts[3].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[3].Type, Is.EqualTo(MessagePartType.Item));

            Assert.That(parts[4].Text, Is.EqualTo("Text5"));
            Assert.That(parts[4].Color, Is.EqualTo(Color.Cyan));
            Assert.That(parts[4].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[4].Type, Is.EqualTo(MessagePartType.Item));

            Assert.That(parts[5].Text, Is.EqualTo("Text6"));
            Assert.That(parts[5].Color, Is.EqualTo(Color.Green));
            Assert.That(parts[5].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[5].Type, Is.EqualTo(MessagePartType.Location));

            Assert.That(parts[6].Text, Is.EqualTo("Text7"));
            Assert.That(parts[6].Color, Is.EqualTo(Color.Green));
            Assert.That(parts[6].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[6].Type, Is.EqualTo(MessagePartType.Location));

            Assert.That(parts[7].Text, Is.EqualTo("Text8"));
            Assert.That(parts[7].Color, Is.EqualTo(Color.Yellow));
            Assert.That(parts[7].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[7].Type, Is.EqualTo(MessagePartType.Player));

            Assert.That(parts[8].Text, Is.EqualTo("Text9"));
            Assert.That(parts[8].Color, Is.EqualTo(Color.Yellow));
            Assert.That(parts[8].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[8].Type, Is.EqualTo(MessagePartType.Player));

            Assert.That(parts[9].Text, Is.EqualTo("Text10"));
            Assert.That(parts[9].Color, Is.EqualTo(Color.Blue));
            Assert.That(parts[9].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[9].Type, Is.EqualTo(MessagePartType.Entrance));
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

            sut.OnMessageReceived += (message) => parts = message.Parts;

            var packet = new PrintJsonPacket {
                Data = new[] { new JsonMessagePart { Type = JsonMessagePartType.PlayerId, Text = "4" } }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

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
            new object[] { ItemFlags.Trap, Color.Salmon }, new object[] { ItemFlags.None, Color.Cyan }
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

            sut.OnMessageReceived += (message) => parts = message.Parts;

            var packet = new PrintJsonPacket {
                Data = new[] {
                    new JsonMessagePart { Type = JsonMessagePartType.ItemId, Text = "1", Flags = itemFlags },
                    new JsonMessagePart { Type = JsonMessagePartType.ItemName, Text = "ItemFive", Flags = itemFlags }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

            Assert.That(parts[0].Text, Is.EqualTo("ItemFour"));
            Assert.That(parts[0].Color, Is.EqualTo(expectedColor));
            Assert.That(parts[0].IsBackgroundColor, Is.EqualTo(false));

            Assert.That(parts[1].Text, Is.EqualTo("ItemFive"));
            Assert.That(parts[1].Color, Is.EqualTo(expectedColor));
            Assert.That(parts[1].IsBackgroundColor, Is.EqualTo(false));
        }

        [Test]
        public void Should_preserve_extra_properties_on_ItemPrintJsonPacket()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(4).Returns("LocalPlayer");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Slot.Returns(4);

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            ItemSendLogMessage logMessage = null;

            sut.OnMessageReceived += (message) => logMessage = message as ItemSendLogMessage;

            var packet = new ItemPrintJsonPacket {
                Data = new[] { new JsonMessagePart { Text = "" } },
                Item = new NetworkItem { Flags = ItemFlags.None, Player = 2, Item = 100, Location = 1000 },
                ReceivingPlayer = 1,
                MessageType = JsonMessageType.ItemSend
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

            Assert.That(logMessage, Is.Not.Null);
            Assert.That(logMessage.Item, Is.EqualTo(packet.Item));
            Assert.That(logMessage.ReceivingPlayerSlot, Is.EqualTo(1));
            Assert.That(logMessage.SendingPlayerSlot, Is.EqualTo(2));
        }

        [Test]
        public void Should_preserve_extra_properties_on_HintPrintJsonPacket()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(4).Returns("LocalPlayer");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Slot.Returns(4);

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            HintItemSendLogMessage logMessage = null;

            sut.OnMessageReceived += (message) => logMessage = message as HintItemSendLogMessage;

            var packet = new HintPrintJsonPacket {
                Data = new[] { new JsonMessagePart { Text = "" } },
                Item = new NetworkItem { Flags = ItemFlags.None, Player = 2, Item = 100, Location = 1000 },
                ReceivingPlayer = 1,
                Found = true,
                MessageType = JsonMessageType.ItemSend
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

            Assert.That(logMessage, Is.Not.Null);
            Assert.That(logMessage.Item, Is.EqualTo(packet.Item));
            Assert.That(logMessage.ReceivingPlayerSlot, Is.EqualTo(1));
            Assert.That(logMessage.SendingPlayerSlot, Is.EqualTo(2));
            Assert.That(logMessage.IsFound, Is.EqualTo(true));
        }

        [Test]
        public void Should_split_new_lines_in_separate_messages_for_print_package()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            var players = Substitute.For<IPlayerHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            List<LogMessage> logMessage = new List<LogMessage>(6);

            sut.OnMessageReceived += (message) => logMessage.Add(message);

            var packet = new PrintPacket {
                Text =
                    "!help \n    Returns the help listing\n!license \n    Returns the licensing information\n!countdown seconds = 10 \n    Start a countdown in seconds"
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

            Assert.That(logMessage.Count, Is.EqualTo(6));
            Assert.That(logMessage[0].ToString(), Is.EqualTo("!help "));
            Assert.That(logMessage[1].ToString(), Is.EqualTo("    Returns the help listing"));
            Assert.That(logMessage[2].ToString(), Is.EqualTo("!license "));
            Assert.That(logMessage[3].ToString(), Is.EqualTo("    Returns the licensing information"));
            Assert.That(logMessage[4].ToString(), Is.EqualTo("!countdown seconds = 10 "));
            Assert.That(logMessage[5].ToString(), Is.EqualTo("    Start a countdown in seconds"));
        }

        [Test]
        public void Should_split_new_lines_in_separate_messages_for_print_json_package()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            var items = Substitute.For<IReceivedItemsHelper>();
            var players = Substitute.For<IPlayerHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            List<LogMessage> logMessage = new List<LogMessage>(3);

            sut.OnMessageReceived += (message) => logMessage.Add(message);

            var packet = new PrintJsonPacket {
                Data = new[] {
                    new JsonMessagePart {
                        Type = JsonMessagePartType.Color,
                        Color = JsonMessagePartColor.Red,
                        Text = "Some text\nover multiple "
                    },
                    new JsonMessagePart { Text = "lines" }
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

            Assert.That(logMessage.Count, Is.EqualTo(2));
            Assert.That(logMessage[0].Parts.Length, Is.EqualTo(1));
            Assert.That(logMessage[0].Parts[0].Text, Is.EqualTo("Some text"));
            Assert.That(logMessage[0].Parts[0].Color, Is.EqualTo(Color.Red));

            Assert.That(logMessage[1].Parts.Length, Is.EqualTo(2));
            Assert.That(logMessage[1].Parts[0].Text, Is.EqualTo("over multiple "));
            Assert.That(logMessage[1].Parts[0].Color, Is.EqualTo(Color.Red));

            Assert.That(logMessage[1].Parts[1].Text, Is.EqualTo("lines"));
            Assert.That(logMessage[1].Parts[1].Type, Is.EqualTo(MessagePartType.Text));
            Assert.That(logMessage[1].Parts[1].Color, Is.EqualTo(Color.White));
        }

        [Test]
        public void Should_not_go_boom_when_datapackage_doesnt_know_certain_values()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var locations = Substitute.For<ILocationCheckHelper>();
            locations.GetLocationNameFromId(Arg.Any<long>()).Returns((string)null);
            var items = Substitute.For<IReceivedItemsHelper>();
            items.GetItemName(Arg.Any<long>()).Returns((string)null);
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(Arg.Any<int>()).Returns((string)null);
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, items, locations, players, connectionInfo);

            MessagePart[] parts = null;

            sut.OnMessageReceived += (message) => parts = message.Parts;

            var packet = new PrintJsonPacket
            {
                Data = new[] {
                    new JsonMessagePart { Type = JsonMessagePartType.ItemId, Text = "123", Flags = ItemFlags.Trap },
                    new JsonMessagePart { Type = JsonMessagePartType.LocationId, Text = "456" },
                    new JsonMessagePart { Type = JsonMessagePartType.PlayerId, Text = "69" },
                }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

            Assert.That(parts.Length, Is.EqualTo(3));
            Assert.That(parts[0].Text, Is.EqualTo("Item: 123"));
            Assert.That(parts[0].Color, Is.EqualTo(Color.Salmon));
            Assert.That(parts[0].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[0].Type, Is.EqualTo(MessagePartType.Item));

            Assert.That(parts[1].Text, Is.EqualTo("Location: 456"));
            Assert.That(parts[1].Color, Is.EqualTo(Color.Green));
            Assert.That(parts[1].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[1].Type, Is.EqualTo(MessagePartType.Location));

            Assert.That(parts[2].Text, Is.EqualTo("Player 69"));
            Assert.That(parts[2].Color, Is.EqualTo(Color.Yellow));
            Assert.That(parts[2].IsBackgroundColor, Is.EqualTo(false));
            Assert.That(parts[2].Type, Is.EqualTo(MessagePartType.Player));
        }
    }
}