using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class DataStorageWrapperFixture
	{
		[Test]
		public void GetHints_should_return_hints_for_current_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Slot.Returns(8);
			connectionInfo.Team.Returns(2);

			var sut = new DataStorageHelper(socket, connectionInfo);

			Hint[] hints = null;

			var availableHints = new []{
				new Hint {
					Entrance = "E", 
					FindingPlayer = 4, 
					Found = false, 
					ItemFlags = ItemFlags.Trap, 
					ItemId = 1337L, 
					ReceivingPlayer = 8
				},
			};

			ExecuteAsyncWithDelay(
				() => { hints = sut.GetHints(); },
				() => RaiseRetrieved(socket, "_read_hints_2_8", JArray.FromObject(availableHints)));

			Assert.IsNotNull(hints);

			Assert.That(hints.Length, Is.EqualTo(availableHints.Length));
			Assert.That(hints[0].Entrance, Is.EqualTo(availableHints[0].Entrance));
			Assert.That(hints[0].FindingPlayer, Is.EqualTo(availableHints[0].FindingPlayer));
			Assert.That(hints[0].Found, Is.EqualTo(availableHints[0].Found));
			Assert.That(hints[0].ItemFlags, Is.EqualTo(availableHints[0].ItemFlags));
			Assert.That(hints[0].ItemId, Is.EqualTo(availableHints[0].ItemId));
			Assert.That(hints[0].ReceivingPlayer, Is.EqualTo(availableHints[0].ReceivingPlayer));
		}

		[Test]
		public void GetHintsAsync_should_return_hints_for_current_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Slot.Returns(7);
			connectionInfo.Team.Returns(3);

			var sut = new DataStorageHelper(socket, connectionInfo);

			Hint[] hints = null;

			var availableHints = new[]{
				new Hint {
					Entrance = null,
					FindingPlayer = 9,
					Found = true,
					ItemFlags = ItemFlags.None,
					ItemId = 1337L,
					ReceivingPlayer = 7
				}
			};

#if NET471
			bool hintsCallbackReceived = false;

			ExecuteAsyncWithDelay(
				() => { 
					sut.GetHintsAsync(t => {
						hintsCallbackReceived = true; 
						hints= t;
					});
				},
				() => RaiseRetrieved(socket, "_read_hints_3_7", JArray.FromObject(availableHints)));

			while (!hintsCallbackReceived)
				Thread.Sleep(10);
#else
			ExecuteAsyncWithDelay(
				() => { hints = sut.GetHintsAsync().Result; },
				() => RaiseRetrieved(socket, "_read_hints_3_7", JArray.FromObject(availableHints)));
#endif
			
			Assert.IsNotNull(hints);

			Assert.That(hints.Length, Is.EqualTo(availableHints.Length));
			Assert.That(hints[0].Entrance, Is.EqualTo(availableHints[0].Entrance));
			Assert.That(hints[0].FindingPlayer, Is.EqualTo(availableHints[0].FindingPlayer));
			Assert.That(hints[0].Found, Is.EqualTo(availableHints[0].Found));
			Assert.That(hints[0].ItemFlags, Is.EqualTo(availableHints[0].ItemFlags));
			Assert.That(hints[0].ItemId, Is.EqualTo(availableHints[0].ItemId));
			Assert.That(hints[0].ReceivingPlayer, Is.EqualTo(availableHints[0].ReceivingPlayer));
		}

		[Test]
		public void GetHints_should_return_null_for_non_existing_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new DataStorageHelper(socket, connectionInfo);
			
			Hint[] hintsBySlot = null;
			Hint[] hintBySlotAndTeam = null;

			ExecuteAsyncWithDelay(
				() => { hintsBySlot = sut.GetHints(11); },
				() => RaiseRetrieved(socket, "_read_hints_0_11", JValue.CreateNull()));
			ExecuteAsyncWithDelay(
				() => { hintBySlotAndTeam = sut.GetHints(11, 2); },
				() => RaiseRetrieved(socket, "_read_hints_2_11", JValue.CreateNull()));

			Assert.Null(hintsBySlot);
			Assert.Null(hintBySlotAndTeam);
		}

		[Test]
		public void GetHintsAsync_should_return_null_for_non_existing_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new DataStorageHelper(socket, connectionInfo);

			Hint[] hintsBySlot = null;
			Hint[] hintBySlotAndTeam = null;

#if NET471
			bool hintsBySlotCallbackReceived = false;
			bool hintBySlotAndTeamCallbackReceived = false;

			ExecuteAsyncWithDelay(
				() => { 
					sut.GetHintsAsync(t => {
						hintsBySlotCallbackReceived = true;
						hintsBySlot = t;
					}, 11);
				},
				() => RaiseRetrieved(socket, "_read_hints_0_11", JValue.CreateNull()));
			ExecuteAsyncWithDelay(
				() => { 
					sut.GetHintsAsync(t => {
						hintBySlotAndTeamCallbackReceived = true;
						hintBySlotAndTeam = t;
					}, 11, 11);
				},
				() => RaiseRetrieved(socket, "_read_hints_11_11", JValue.CreateNull()));

			while (!hintsBySlotCallbackReceived || !hintBySlotAndTeamCallbackReceived)
				Thread.Sleep(10);
#else
			ExecuteAsyncWithDelay(
				() => { hintsBySlot = sut.GetHintsAsync(11).Result; },
				() => RaiseRetrieved(socket, "_read_hints_0_11", JValue.CreateNull()));
			ExecuteAsyncWithDelay(
				() => { hintBySlotAndTeam = sut.GetHintsAsync(11, 11).Result; },
				() => RaiseRetrieved(socket, "_read_hints_11_11", JValue.CreateNull()));
#endif

			Assert.Null(hintsBySlot);
			Assert.Null(hintBySlotAndTeam);
		}

		[Test]
		public void GetSlotData_should_return_slot_data_for_current_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Slot.Returns(4);

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, object> slotData = null;

			var serverSlotData = new Dictionary<string, object> {
				{ "One", 1 }, 
				{ "Two", 2 }
			};

			ExecuteAsyncWithDelay(
				() => { slotData = sut.GetSlotData(); },
				() => RaiseRetrieved(socket, "_read_slot_data_4", JObject.FromObject(serverSlotData)));

			Assert.IsNotNull(slotData);

			Assert.That(slotData["One"], Is.EqualTo(1)); 
			Assert.That(slotData["Two"], Is.EqualTo(2));
		}

		[Test]
		public void GetSlotData_should_return_null_for_non_existing_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, object> slotData = null;

			ExecuteAsyncWithDelay(
				() => { slotData = sut.GetSlotData(99); },
				() => RaiseRetrieved(socket, "_read_slot_data_99", JValue.CreateNull()));

			Assert.IsNull(slotData);
		}

		[Test]
		public void GetSlotDataAsync_should_return_slot_data_for_current_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Slot.Returns(88);

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, object> slotData = null;

			var serverSlotData = new Dictionary<string, object> {
				{ "One", 1 },
				{ "Two", 2 }
			};
			
#if NET471
			bool slotDataCallbackReceived = false;

			ExecuteAsyncWithDelay(
				() => {
					sut.GetSlotDataAsync(t => {
						slotDataCallbackReceived = true;
						slotData = t;
					});
				},
				() => RaiseRetrieved(socket, "_read_slot_data_88", JObject.FromObject(serverSlotData)));

			while (!slotDataCallbackReceived)
				Thread.Sleep(10);
#else
			ExecuteAsyncWithDelay(
				() => { slotData = sut.GetSlotDataAsync().Result; },
				() => RaiseRetrieved(socket, "_read_slot_data_88", JObject.FromObject(serverSlotData)));
#endif

			Assert.IsNotNull(slotData);

			Assert.That(slotData["One"], Is.EqualTo(1));
			Assert.That(slotData["Two"], Is.EqualTo(2));
		}

		[Test]
		public void GetSlotDataAsync_should_return_null_for_non_existing_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, object> slotData = null;

#if NET471
			bool slotDataCallbackReceived = false;

			ExecuteAsyncWithDelay(
				() => {
					sut.GetSlotDataAsync(t => {
						slotDataCallbackReceived = true;
						slotData = t;
					}, 88);
				},
				() => RaiseRetrieved(socket, "_read_slot_data_88", JValue.CreateNull()));

			while (!slotDataCallbackReceived)
				Thread.Sleep(10);
#else
			ExecuteAsyncWithDelay(
				() => { slotData = sut.GetSlotDataAsync(88).Result; },
				() => RaiseRetrieved(socket, "_read_slot_data_88", JValue.CreateNull()));
#endif

			Assert.IsNull(slotData);
		}

		[Test]
		public void GetItemNameGroups_should_return_item_name_groups_for_current_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Game.Returns("MY Game");

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, string[]> slotData = null;

			var serverItemNameGroups = new Dictionary<string, string[]> {
				{ "Group1", new[] { "A", "B" } },
				{ "Group3", new[] { "Q", "B" } },
			};

			ExecuteAsyncWithDelay(
				() => { slotData = sut.GetItemNameGroups(); },
				() => RaiseRetrieved(socket, "_read_item_name_groups_MY Game", JObject.FromObject(serverItemNameGroups)));

			Assert.IsNotNull(slotData);

			Assert.That(slotData["Group1"][0], Is.EqualTo("A"));
			Assert.That(slotData["Group1"][1], Is.EqualTo("B"));
			Assert.That(slotData["Group3"][0], Is.EqualTo("Q"));
			Assert.That(slotData["Group3"][1], Is.EqualTo("B"));
		}

		[Test]
		public void GetItemNameGroups_should_return_null_for_non_existing_game()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, string[]> itemNameGroups = null;

			ExecuteAsyncWithDelay(
				() => { itemNameGroups = sut.GetItemNameGroups("NOT A REAL GAME"); },
				() => RaiseRetrieved(socket, "_read_item_name_groups_NOT A REAL GAME", JValue.CreateNull()));

			Assert.IsNull(itemNameGroups);
		}

		[Test]
		public void GetItemNameGroupsAsync_should_return_item_name_groups_for_current_player_slot()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Game.Returns("Below Zero");

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, string[]> itemNameGroups = null;

			var serverItemNameGroups = new Dictionary<string, string[]> {
				{ "Group1", new[] { "A", "B" } },
				{ "Group3", new[] { "Q", "B" } },
			};

#if NET471
			bool itemNameGroupsCallbackReceived = false;

			ExecuteAsyncWithDelay(
				() => {
					sut.GetItemNameGroupsAsync(t => {
						itemNameGroupsCallbackReceived = true;
						itemNameGroups = t;
					});
				},
				() => RaiseRetrieved(socket, "_read_item_name_groups_Below Zero", JObject.FromObject(serverItemNameGroups)));

			while (!itemNameGroupsCallbackReceived)
				Thread.Sleep(10);
#else
			ExecuteAsyncWithDelay(
				() => { itemNameGroups = sut.GetItemNameGroupsAsync().Result; },
				() => RaiseRetrieved(socket, "_read_item_name_groups_Below Zero", JObject.FromObject(serverItemNameGroups)));
#endif

			Assert.IsNotNull(itemNameGroups);

			Assert.That(itemNameGroups["Group1"][0], Is.EqualTo("A"));
			Assert.That(itemNameGroups["Group1"][1], Is.EqualTo("B"));
			Assert.That(itemNameGroups["Group3"][0], Is.EqualTo("Q"));
			Assert.That(itemNameGroups["Group3"][1], Is.EqualTo("B"));
		}

		[Test]
		public void GetItemNameGroupsAsync_should_return_null_for_non_existing_game()
		{
			var socket = Substitute.For<IArchipelagoSocketHelper>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new DataStorageHelper(socket, connectionInfo);

			Dictionary<string, string[]> itemNameGroups = null;

#if NET471
			bool slotDataCallbackReceived = false;

			ExecuteAsyncWithDelay(
				() => {
					sut.GetItemNameGroupsAsync(t => {
						slotDataCallbackReceived = true;
						itemNameGroups = t;
					}, "YoloTheGame");
				},
				() => RaiseRetrieved(socket, "_read_item_name_groups_YoloTheGame", JValue.CreateNull()));

			while (!slotDataCallbackReceived)
				Thread.Sleep(10);
#else
			ExecuteAsyncWithDelay(
				() => { itemNameGroups = sut.GetItemNameGroupsAsync("YoloTheGame").Result; },
				() => RaiseRetrieved(socket, "_read_item_name_groups_YoloTheGame", JValue.CreateNull()));
#endif

			Assert.IsNull(itemNameGroups);
		}

		public static void ExecuteAsyncWithDelay(Action retrieve, Action raiseEvent)
		{
			Task.WaitAll(new[] {
				Task.Factory.StartNew(retrieve),
				Task.Factory.StartNew(() =>
				{
					Thread.Sleep(100);
					raiseEvent();
				})
			}, 3000);
		}

		public static void RaiseRetrieved(IArchipelagoSocketHelper socket, string key, JToken value)
		{
			var packet = new RetrievedPacket() { Data = new Dictionary<string, JToken> { { key, value } } };

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(packet);
		}
	}
}
