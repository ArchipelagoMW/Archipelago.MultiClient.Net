using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class MessageLogHelperFixture
    {
        [Test]
        public void Should_convert_print_json_packet_into_string()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			itemInfoResolver.GetLocationName(6L).Returns("Text6");
            var items = Substitute.For<IReceivedItemsHelper>();
            items.GetItemName(4L).Returns("Text4");
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(8).Returns("Text8");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

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
        public void Should_get_parsed_data_for_print_json_packet()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var itemInfoResolver = Substitute.For<IItemInfoResolver>();
            itemInfoResolver.GetLocationName(6L).Returns("Text6");
            var items = Substitute.For<IReceivedItemsHelper>();
            items.GetItemName(4L).Returns("Text4");
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(8).Returns("Text8");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Slot.Returns(0);

            var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

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
            var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(4).Returns("LocalPlayer");
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Slot.Returns(4);

            var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

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
            var itemInfoResolver = Substitute.For<IItemInfoResolver>();
            itemInfoResolver.GetItemName(1L).Returns("ItemFour");
            var players = Substitute.For<IPlayerHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

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
        public void Should_split_new_lines_in_separate_messages_for_print_json_package()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

            var logMessage = new List<LogMessage>(3);

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
            var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			itemInfoResolver.GetLocationName(Arg.Any<long>()).Returns((string)null);
            itemInfoResolver.GetItemName(Arg.Any<long>()).Returns((string)null);
            var players = Substitute.For<IPlayerHelper>();
            players.GetPlayerAlias(Arg.Any<int>()).Returns((string)null);
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

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

		[Test]
		public void Should_preserve_extra_properties_on_ItemPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");

			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			ItemSendLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as ItemSendLogMessage;

			var packet = new ItemPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Item = new NetworkItem { Flags = ItemFlags.None, Player = 3, Item = 100, Location = 1000 },
				ReceivingPlayer = 5,
				MessageType = JsonMessageType.ItemSend
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);
			Assert.That(logMessage.Item, Is.EqualTo(packet.Item));

			Assert.That(logMessage.Receiver.Slot, Is.EqualTo(5));
			Assert.That(logMessage.Sender.Slot, Is.EqualTo(3));

			Assert.That(logMessage.IsReceiverTheActivePlayer, Is.EqualTo(true));
			Assert.That(logMessage.IsSenderTheActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(true));

			Assert.That(logMessage.ReceivingPlayerSlot, Is.EqualTo(5));
			Assert.That(logMessage.SendingPlayerSlot, Is.EqualTo(3));
		}

		[Test]
		public void Should_preserve_extra_properties_on_ItemCheatPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(2).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(2);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			ItemCheatLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as ItemCheatLogMessage;

			var packet = new ItemCheatPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Item = new NetworkItem { Flags = ItemFlags.None, Player = 1, Item = 100, Location = 1000 },
				ReceivingPlayer = 1,
				Team = 0,
				MessageType = JsonMessageType.ItemCheat
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);
			Assert.That(logMessage.Item, Is.EqualTo(packet.Item));

			Assert.That(logMessage.Receiver.Slot, Is.EqualTo(1));
			Assert.That(logMessage.Sender.Slot, Is.EqualTo(0)); //Slot 0 = Server
			Assert.That(logMessage.Sender.Name, Is.EqualTo("Server"));
			Assert.That(logMessage.Sender.Game, Is.EqualTo("Archipelago"));

			Assert.That(logMessage.IsReceiverTheActivePlayer, Is.EqualTo(false));
			Assert.That(logMessage.IsSenderTheActivePlayer, Is.EqualTo(false));
			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(false));
		}

		[Test]
		public void Should_preserve_extra_properties_on_HintPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			HintItemSendLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as HintItemSendLogMessage;

			var packet = new HintPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Item = new NetworkItem { Flags = ItemFlags.None, Player = 3, Item = 100, Location = 1000 },
				ReceivingPlayer = 2,
				Found = true,
				MessageType = JsonMessageType.Hint
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);
			Assert.That(logMessage.Item, Is.EqualTo(packet.Item));

			Assert.That(logMessage.Receiver.Slot, Is.EqualTo(2));
			Assert.That(logMessage.Sender.Slot, Is.EqualTo(3));

			Assert.That(logMessage.IsReceiverTheActivePlayer, Is.EqualTo(false));
			Assert.That(logMessage.IsSenderTheActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsFound, Is.EqualTo(true));

			Assert.That(logMessage.ReceivingPlayerSlot, Is.EqualTo(2));
			Assert.That(logMessage.SendingPlayerSlot, Is.EqualTo(3));
		}

		[Test]
		public void Should_preserve_extra_properties_on_JoinPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			JoinLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as JoinLogMessage;

			var packet = new JoinPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Team = 0,
				Slot = 3,
				Tags = new []{ "TAG" },
				MessageType = JsonMessageType.Join
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Player.Team, Is.EqualTo(0));
			Assert.That(logMessage.Player.Slot, Is.EqualTo(3));

			Assert.That(logMessage.Tags, Is.EquivalentTo(new[] { "TAG" }));

			Assert.That(logMessage.IsActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(false));
		}

		[Test]
		public void Should_preserve_extra_properties_on_LeavePrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			LeaveLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as LeaveLogMessage;

			var packet = new LeavePrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Team = 0,
				Slot = 5,
				MessageType = JsonMessageType.Part
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Player.Team, Is.EqualTo(0));
			Assert.That(logMessage.Player.Slot, Is.EqualTo(5));

			Assert.That(logMessage.IsActivePlayer, Is.EqualTo(true));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(true));
		}

		[Test]
		public void Should_preserve_extra_properties_on_ChatPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1, Groups = new []{ new NetworkSlot { GroupMembers = new []{ 2,3,5 } } } },
				new PlayerInfo { Team = 0, Slot = 2, Groups = new []{ new NetworkSlot { GroupMembers = new []{ 1,3,5 } } } },
				new PlayerInfo { Team = 0, Slot = 3, Groups = new []{ new NetworkSlot { GroupMembers = new []{ 1,2,5 } } } },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5, Groups = new []{ new NetworkSlot { GroupMembers = new []{ 1,2,3 } } } }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			ChatLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as ChatLogMessage;

			var packet = new ChatPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Team = 0,
				Slot = 2,
				Message = "Silly duplicated data",
				MessageType = JsonMessageType.Chat
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Player.Team, Is.EqualTo(0));
			Assert.That(logMessage.Player.Slot, Is.EqualTo(2));

			Assert.That(logMessage.IsActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(true));

			Assert.That(logMessage.Message, Is.EqualTo("Silly duplicated data"));
		}

		[Test]
		public void Should_preserve_extra_properties_on_ServerChatPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			ServerChatLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as ServerChatLogMessage;

			var packet = new ServerChatPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Message = "Silly duplicated data",
				MessageType = JsonMessageType.ServerChat
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Message, Is.EqualTo("Silly duplicated data"));
		}

		[Test]
		public void Should_preserve_extra_properties_on_TutorialPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			TutorialLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as TutorialLogMessage;

			var packet = new TutorialPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				MessageType = JsonMessageType.Tutorial
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);
		}

		[Test]
		public void Should_preserve_extra_properties_on_TagsChangedPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			TagsChangedLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as TagsChangedLogMessage;

			var packet = new TagsChangedPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Team = 0,
				Slot = 3,
				Tags = new[] { "TAG", "TAG2" },
				MessageType = JsonMessageType.TagsChanged
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Player.Team, Is.EqualTo(0));
			Assert.That(logMessage.Player.Slot, Is.EqualTo(3));

			Assert.That(logMessage.Tags, Is.EquivalentTo(new[] { "TAG", "TAG2" }));

			Assert.That(logMessage.IsActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(false));
		}

		[Test]
		public void Should_preserve_extra_properties_on_CommandResultPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			CommandResultLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as CommandResultLogMessage;

			var packet = new CommandResultPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				MessageType = JsonMessageType.CommandResult
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);
		}

		[Test]
		public void Should_preserve_extra_properties_on_AdminCommandResultPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			AdminCommandResultLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as AdminCommandResultLogMessage;

			var packet = new AdminCommandResultPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				MessageType = JsonMessageType.CommandResult
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);
		}

		[Test]
		public void Should_preserve_extra_properties_on_GoalPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			GoalLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as GoalLogMessage;

			var packet = new GoalPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Team = 0,
				Slot = 3,
				MessageType = JsonMessageType.TagsChanged
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Player.Team, Is.EqualTo(0));
			Assert.That(logMessage.Player.Slot, Is.EqualTo(3));

			Assert.That(logMessage.IsActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(false));
		}

		[Test]
		public void Should_preserve_extra_properties_on_ReleasePrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			ReleaseLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as ReleaseLogMessage;

			var packet = new ReleasePrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Team = 0,
				Slot = 3,
				MessageType = JsonMessageType.TagsChanged
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Player.Team, Is.EqualTo(0));
			Assert.That(logMessage.Player.Slot, Is.EqualTo(3));

			Assert.That(logMessage.IsActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(false));
		}

		[Test]
		public void Should_preserve_extra_properties_on_CollectPrintJsonPacket()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var players = Substitute.For<IPlayerHelper>();
			players.GetPlayerAlias(5).Returns("LocalPlayer");
			players.Players.Returns(GetPlayerCollection(new List<PlayerInfo> {
				new PlayerInfo { Team = 0, Slot = 1 },
				new PlayerInfo { Team = 0, Slot = 2 },
				new PlayerInfo { Team = 0, Slot = 3 },
				new PlayerInfo { Team = 0, Slot = 4 },
				new PlayerInfo { Team = 0, Slot = 5 }
			}));
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Team.Returns(0);
			connectionInfo.Slot.Returns(5);

			var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

			CollectLogMessage logMessage = null;

			sut.OnMessageReceived += (message) => logMessage = message as CollectLogMessage;

			var packet = new CollectPrintJsonPacket
			{
				Data = new[] { new JsonMessagePart { Text = "" } },
				Team = 0,
				Slot = 3,
				MessageType = JsonMessageType.TagsChanged
			};

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

			Assert.That(logMessage, Is.Not.Null);

			Assert.That(logMessage.Player.Team, Is.EqualTo(0));
			Assert.That(logMessage.Player.Slot, Is.EqualTo(3));

			Assert.That(logMessage.IsActivePlayer, Is.EqualTo(false));

			Assert.That(logMessage.IsRelatedToActivePlayer, Is.EqualTo(false));
		}
		
		[Test]
        public void Should_preserve_extra_properties_on_CountdownPrintJsonPacket()
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var itemInfoResolver = Substitute.For<IItemInfoResolver>();
	        var players = Substitute.For<IPlayerHelper>();
	        var connectionInfo = Substitute.For<IConnectionInfoProvider>();

	        var sut = new MessageLogHelper(socket, itemInfoResolver, players, connectionInfo);

	        CountdownLogMessage logMessage = null;

	        sut.OnMessageReceived += (message) =>
		        logMessage = message as CountdownLogMessage;

	        var packet = new CountdownPrintJsonPacket
	        {
		        Data = new[] { new JsonMessagePart { Text = "" } },
		        RemainingSeconds = 8,
		        MessageType = JsonMessageType.Countdown
	        };

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);

	        Assert.That(logMessage, Is.Not.Null);
	        Assert.That(logMessage.RemainingSeconds, Is.EqualTo(8));
        }

#if NET471 || NET472
	    static Dictionary<int, ReadOnlyCollection<PlayerInfo>> GetPlayerCollection(IList<PlayerInfo> playerInfos) => 
		    new Dictionary<int, ReadOnlyCollection<PlayerInfo>>(
#else
	    static ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>> GetPlayerCollection(IList<PlayerInfo> playerInfos) =>
		    new ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>>(
#endif
				new Dictionary<int, ReadOnlyCollection<PlayerInfo>> { {
				    0, new ReadOnlyCollection<PlayerInfo>(
						new List<PlayerInfo>(new[] {
							new PlayerInfo {
								Team = 0, 
								Slot = 0, 
								Name = "Server", 
								Alias = "Server", 
								Game = "Archipelago", 
								Groups = new NetworkSlot[0]
							}
						})
						.Concat(playerInfos)
						.ToList()
				)}});
	}
}