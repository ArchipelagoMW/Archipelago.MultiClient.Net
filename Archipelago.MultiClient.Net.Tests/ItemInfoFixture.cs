using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using NSubstitute;
using NUnit.Framework;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class ItemInfoFixture
	{
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(true, true, false)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		[TestCase(false, true, false)]
		public void Should_serialize_and_deserialize_FullItemInfo(bool scouted, bool fullJson, bool provideSession)
		{
			var receiverGame = "ReceiverGame";
			var senderGame = "SenderGame";

			var networkItem = new NetworkItem() { Item = 20L, Location = 50L, Player = 3, Flags = ItemFlags.NeverExclude };

			var sender = new PlayerInfo {
				Team = 20,
				Slot = 3,
				Alias = "Not defined",
				Game = scouted ? receiverGame : senderGame,
				Groups = new[] {
					new NetworkSlot {
						Game = "Archipelago",
						GroupMembers = new[] { 4, 5, 600 },
						Name = "ItemSink",
						Type = SlotType.Group
					}
				}
			};

			var currentPlayer = new PlayerInfo
			{
				Team = 20,
				Slot = 7,
				Alias = "Current",
				Game = scouted ? senderGame : receiverGame,
				Groups = new[] {
					new NetworkSlot {
						Game = "Archipelago",
						GroupMembers = new[] { 1 },
						Name = "CurrentP",
						Type = SlotType.Player
					}
				}
			};

			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var connectionInfoProvider = Substitute.For<IConnectionInfoProvider>();
			connectionInfoProvider.Slot.Returns(7);
			connectionInfoProvider.Team.Returns(20);
			var playerInfoProvider = Substitute.For<IPlayerHelper>();
			playerInfoProvider.GetPlayerInfo(connectionInfoProvider.Team, networkItem.Player).Returns(sender);
			playerInfoProvider.GetPlayerInfo(connectionInfoProvider.Team, connectionInfoProvider.Slot).Returns(currentPlayer);
			var session = CreateTestSession(itemInfoResolver, playerInfoProvider, connectionInfoProvider);

			var sut = scouted
				? new ScoutedItemInfo(networkItem, receiverGame, senderGame, itemInfoResolver, currentPlayer)
				: new ItemInfo(networkItem, receiverGame, senderGame, itemInfoResolver, sender);
				
			var json = sut.ToSerializable().ToJson(fullJson);
			var deserialized = SerializableItemInfo.FromJson(json, provideSession ? session : null);

			Assert.IsNotNull(deserialized);
			Assert.That(deserialized.ItemId, Is.EqualTo(sut.ItemId));
			Assert.That(deserialized.LocationId, Is.EqualTo(sut.LocationId));
			Assert.That(deserialized.PlayerSlot, Is.EqualTo(sut.Player.Slot));
			Assert.That(deserialized.Flags, Is.EqualTo(sut.Flags));
			Assert.That(deserialized.ItemGame, Is.EqualTo(sut.ItemGame));
			Assert.That(deserialized.LocationGame, Is.EqualTo(sut.LocationGame));
		}

		IArchipelagoSession CreateTestSession(IItemInfoResolver itemInfoResolver, IPlayerHelper playerInfoProvider, IConnectionInfoProvider connectionInfo)
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();

			var locations = new LocationCheckHelper(socket, itemInfoResolver, connectionInfo, playerInfoProvider);
			var item = new ReceivedItemsHelper(socket, locations, itemInfoResolver, connectionInfo, playerInfoProvider);

			var session = Substitute.For<IArchipelagoSession>();
			session.Players.Returns(playerInfoProvider);
			session.ConnectionInfo.Returns(connectionInfo);
			session.Locations.Returns(locations);
			session.Items.Returns(item);

			return session;
		}
	}
}


