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
		[TestCase(true)]
		[TestCase(false)]
		public void Should_convert_to_serializeable(bool scouted)
		{
			var remotePlayerGame = "Satisfactory";
			var myGame = "Timespinner";

			var networkItem = new NetworkItem { Item = 20L, Location = 50L, Player = 3, Flags = ItemFlags.NeverExclude };

			var remotePlayer = new PlayerInfo
			{
				Team = 20,
				Slot = 3,
				Name = "Piet",
				Alias = "Not defined",
				Game = remotePlayerGame,
				Groups = new[] {
					new NetworkSlot {
						Game = "Archipelago",
						GroupMembers = new[] { 4, 5, 600 },
						Name = "ItemSink",
						Type = SlotType.Group
					}
				}
			};

			var playerHelper = Substitute.For<IPlayerHelper>();
			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			itemInfoResolver.GetItemName(networkItem.Item, scouted ? remotePlayerGame : myGame).Returns("ItemToSendName");
			itemInfoResolver.GetLocationName(networkItem.Location, scouted ? myGame : remotePlayerGame).Returns("LocationOfItemName");

			var sut = scouted
				? new ScoutedItemInfo(networkItem, remotePlayerGame, myGame, itemInfoResolver, playerHelper, remotePlayer)
				: new ItemInfo(networkItem, myGame, remotePlayerGame, itemInfoResolver, remotePlayer);

			var serializable = sut.ToSerializable();

			Assert.IsNotNull(serializable);
			Assert.That(serializable.IsScout, Is.EqualTo(scouted));
			Assert.That(serializable.ItemId, Is.EqualTo(sut.ItemId));
			Assert.That(serializable.LocationId, Is.EqualTo(sut.LocationId));
			Assert.That(serializable.PlayerSlot, Is.EqualTo(sut.Player.Slot));
			Assert.That(serializable.Player, Is.Not.Null);
			Assert.That(serializable.Player.Slot, Is.EqualTo(sut.Player.Slot));
			Assert.That(serializable.Player.Team, Is.EqualTo(sut.Player.Team));
			Assert.That(serializable.Player.Game, Is.EqualTo(sut.Player.Game));
			Assert.That(serializable.Player.Name, Is.EqualTo(sut.Player.Name));
			Assert.That(serializable.Player.Alias, Is.EqualTo(sut.Player.Alias));
			Assert.That(serializable.Player.Groups.Length, Is.EqualTo(sut.Player.Groups.Length));
			Assert.That(serializable.Flags, Is.EqualTo(sut.Flags));
			Assert.That(serializable.ItemGame, Is.EqualTo(sut.ItemGame));
			Assert.That(serializable.ItemName, Is.EqualTo(sut.ItemName));
			Assert.That(serializable.ItemDisplayName, Is.EqualTo(sut.ItemDisplayName));
			Assert.That(serializable.LocationGame, Is.EqualTo(sut.LocationGame));
			Assert.That(serializable.LocationName, Is.EqualTo(sut.LocationName));
			Assert.That(serializable.LocationDisplayName, Is.EqualTo(sut.LocationDisplayName));
		}

		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(true, true, false)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		[TestCase(false, true, false)]
		public void Should_serialize_and_deserialize_variations_of_ItemInfo(bool scouted, bool fullJson, bool provideSession)
		{
			var remotePlayerGame = "Satisfactory";
			var myGame = "Timespinner";
			
			var sut = new SerializableItemInfo {
				IsScout = scouted,
				ItemId = 20L,
				LocationId = 50L,
				PlayerSlot = 3,
				Player = new PlayerInfo {
					Team = 20,
					Slot = 3,
					Name = "RemotePlayer",
					Alias = "Not defined",
					Game = remotePlayerGame,
					Groups = new[] {
						new NetworkSlot {
							Game = "Archipelago",
							GroupMembers = new[] { 4, 5, 600 },
							Name = "ItemSink",
							Type = SlotType.Group
						}
					}
				},
				ItemGame = scouted ? remotePlayerGame : myGame,
				ItemName = "ItemToSendName",
				LocationGame = scouted ? myGame : remotePlayerGame,
				LocationName = "LocationOfItemName"
			};

			var json = sut.ToJson(fullJson);
			var deserialized = SerializableItemInfo.FromJson(json, 
				provideSession 
					? CreateTestSession(scouted, remotePlayerGame, myGame, sut) 
					: null);

			Assert.IsNotNull(deserialized);
			Assert.That(deserialized.IsScout, Is.EqualTo(scouted));
			Assert.That(deserialized.ItemId, Is.EqualTo(sut.ItemId));
			Assert.That(deserialized.LocationId, Is.EqualTo(sut.LocationId));
			Assert.That(deserialized.PlayerSlot, Is.EqualTo(sut.Player.Slot));
			Assert.That(deserialized.Player, Is.Not.Null);
			Assert.That(deserialized.Player.Slot, Is.EqualTo(sut.Player.Slot));
			Assert.That(deserialized.Player.Team, Is.EqualTo(sut.Player.Team));
			Assert.That(deserialized.Player.Game, Is.EqualTo(sut.Player.Game));
			Assert.That(deserialized.Player.Name, Is.EqualTo(sut.Player.Name));
			Assert.That(deserialized.Player.Alias, Is.EqualTo(sut.Player.Alias));
			Assert.That(deserialized.Player.Groups.Length, Is.EqualTo(sut.Player.Groups.Length));
			Assert.That(deserialized.Flags, Is.EqualTo(sut.Flags));
			Assert.That(deserialized.ItemGame, Is.EqualTo(sut.ItemGame));
			Assert.That(deserialized.ItemName, Is.EqualTo(sut.ItemName));
			Assert.That(deserialized.ItemDisplayName, Is.EqualTo(sut.ItemDisplayName));
			Assert.That(deserialized.LocationGame, Is.EqualTo(sut.LocationGame));
			Assert.That(deserialized.LocationName, Is.EqualTo(sut.LocationName));
			Assert.That(deserialized.LocationDisplayName, Is.EqualTo(sut.LocationDisplayName));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_deserialize_partial_item_info_with_no_session_provided_accepting_the_loss_of_context(bool scouted)
		{
			var remotePlayerGame = "Satisfactory";
			var myGame = "Timespinner";

			var sut = new SerializableItemInfo
			{
				IsScout = scouted,
				ItemId = 20L,
				LocationId = 50L,
				PlayerSlot = 3,
				Player = new PlayerInfo
				{
					Team = 20,
					Slot = 3,
					Name = "RemotePlayer",
					Alias = "Not defined",
					Game = remotePlayerGame,
					Groups = new[] {
						new NetworkSlot {
							Game = "Archipelago",
							GroupMembers = new[] { 4, 5, 600 },
							Name = "ItemSink",
							Type = SlotType.Group
						}
					}
				},
				ItemGame = scouted ? remotePlayerGame : myGame,
				ItemName = "ItemToSendName",
				LocationGame = scouted ? myGame : remotePlayerGame,
				LocationName = "LocationOfItemName"
			};

			var json = sut.ToJson();
			var deserialized = SerializableItemInfo.FromJson(json);

			Assert.IsNotNull(deserialized);
			Assert.That(deserialized.IsScout, Is.EqualTo(scouted));
			Assert.That(deserialized.ItemId, Is.EqualTo(sut.ItemId));
			Assert.That(deserialized.LocationId, Is.EqualTo(sut.LocationId));
			Assert.That(deserialized.PlayerSlot, Is.EqualTo(sut.Player.Slot));
			Assert.That(deserialized.Player, Is.Null);
			Assert.That(deserialized.Flags, Is.EqualTo(sut.Flags));

			if (scouted)
			{
				Assert.That(deserialized.ItemGame, Is.EqualTo(sut.ItemGame));
				Assert.That(deserialized.ItemName, Is.Null);
				Assert.That(deserialized.ItemDisplayName, Is.EqualTo($"Item: {deserialized.ItemId}"));

				Assert.That(deserialized.LocationGame, Is.Null);
				Assert.That(deserialized.LocationName, Is.Null);
				Assert.That(deserialized.LocationDisplayName, Is.EqualTo($"Location: {deserialized.LocationId}"));
			}
			else
			{
				Assert.That(deserialized.ItemGame, Is.Null);
				Assert.That(deserialized.ItemName, Is.Null);
				Assert.That(deserialized.ItemDisplayName, Is.EqualTo($"Item: {deserialized.ItemId}"));
				
				Assert.That(deserialized.LocationGame, Is.EqualTo(sut.LocationGame));
				Assert.That(deserialized.LocationName, Is.Null);
				Assert.That(deserialized.LocationDisplayName, Is.EqualTo($"Location: {deserialized.LocationId}"));
			}
		}

		[Test]
		public void Scout_should_be_related_when_player_is_active_player()
		{
			var activePlayer = new PlayerInfo(1, 1, "Me", "Myself", "Hollow Knight", [], []);

			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var playerHelper = Substitute.For<IPlayerHelper>();
			playerHelper.ActivePlayer.Returns(activePlayer);

			var scoutedItem = new ScoutedItemInfo(new NetworkItem
			{
				Flags = ItemFlags.None,
				Item = 10,
				Location = 20,
				Player = 1
			}, "Hollow Knight", "Hollow Knight", itemInfoResolver, playerHelper, activePlayer);

			Assert.That(scoutedItem.IsReceiverRelatedToActivePlayer, Is.True);
		}

		[Test]
		public void Scout_should_be_related_when_player_is_relevant_group()
		{
			var networkGroup = new NetworkSlot
			{
				Game = "Hollow Knight",
				Name = "Everygrubby",
				Type = SlotType.Group,
				GroupMembers = [1, 2]
			};
			var activePlayer = new PlayerInfo(1, 1, "Me", "Myself", "Hollow Knight", [networkGroup], []);
			var group = new PlayerInfo(1, 3, "Everygrubby", "Everygrubby", "Hollow Knight", [], [1, 2]);

			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var playerHelper = Substitute.For<IPlayerHelper>();
			playerHelper.ActivePlayer.Returns(activePlayer);
			playerHelper.GetPlayerInfo(1, 1).Returns(activePlayer);
			playerHelper.GetPlayerInfo(1, 3).Returns(group);

			var scoutedItem = new ScoutedItemInfo(new NetworkItem
			{
				Flags = ItemFlags.None,
				Item = 10,
				Location = 20,
				Player = 3
			}, "Hollow Knight", "Hollow Knight", itemInfoResolver, playerHelper, group);

			Assert.That(scoutedItem.IsReceiverRelatedToActivePlayer, Is.True);
		}

		[Test]
		public void Scout_should_not_be_related_when_player_is_shared_itemlink()
		{
			var networkGroup = new NetworkSlot
			{
				Game = "Hollow Knight",
				Name = "Everygrubby",
				Type = SlotType.Group,
				GroupMembers = [1, 2]
			};
			var activePlayer = new PlayerInfo(1, 1, "Me", "Myself", "Hollow Knight", [networkGroup], []);
			var otherPlayer = new PlayerInfo(1, 2, "Other", "Other", "Hollow Knight", [networkGroup], []);
			var group = new PlayerInfo(1, 3, "Everygrubby", "Everygrubby", "Hollow Knight", [], [1, 2]);

			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			var playerHelper = Substitute.For<IPlayerHelper>();
			playerHelper.ActivePlayer.Returns(activePlayer);
			playerHelper.GetPlayerInfo(1, 2).Returns(group);
			playerHelper.GetPlayerInfo(1, 3).Returns(group);

			var scoutedItem = new ScoutedItemInfo(new NetworkItem
			{
				Flags = ItemFlags.None,
				Item = 10,
				Location = 20,
				Player = 2
			}, "Hollow Knight", "Hollow Knight", itemInfoResolver, playerHelper, otherPlayer);

			Assert.That(scoutedItem.IsReceiverRelatedToActivePlayer, Is.False);
		}

		static IArchipelagoSession CreateTestSession(bool scouted, string remotePlayerGame, string myGame, SerializableItemInfo itemInfo)
		{
			var currentPlayer = new PlayerInfo
			{
				Team = 20,
				Slot = 7,
				Name = "Me",
				Alias = "Current",
				Game = myGame,
				Groups = new[] {
					new NetworkSlot {
						Game = myGame,
						GroupMembers = new[] { 1 },
						Name = "CurrentP",
						Type = SlotType.Player
					}
				}
			};

			var itemInfoResolver = Substitute.For<IItemInfoResolver>();
			itemInfoResolver.GetItemName(itemInfo.ItemId, scouted ? remotePlayerGame : myGame).Returns(itemInfo.ItemName);
			itemInfoResolver.GetLocationName(itemInfo.LocationId, scouted ? myGame : remotePlayerGame).Returns(itemInfo.LocationName);
			var connectionInfoProvider = Substitute.For<IConnectionInfoProvider>();
			connectionInfoProvider.Slot.Returns(currentPlayer.Slot);
			connectionInfoProvider.Team.Returns(currentPlayer.Team);
			connectionInfoProvider.Game.Returns(currentPlayer.Game);
			var playerInfoProvider = Substitute.For<IPlayerHelper>();
			playerInfoProvider.GetPlayerInfo(itemInfo.Player.Slot).Returns(itemInfo.Player);
			playerInfoProvider.GetPlayerInfo(currentPlayer.Slot).Returns(currentPlayer);
			
			var socket = Substitute.For<IArchipelagoSocketHelper>();

			var locations = new LocationCheckHelper(socket, itemInfoResolver, connectionInfoProvider, playerInfoProvider);
			var item = new ReceivedItemsHelper(socket, locations, itemInfoResolver, connectionInfoProvider, playerInfoProvider);

			var session = Substitute.For<IArchipelagoSession>();
			session.Players.Returns(playerInfoProvider);
			session.ConnectionInfo.Returns(connectionInfoProvider);
			session.Locations.Returns(locations);
			session.Items.Returns(item);

			return session;
		}
	}
}


