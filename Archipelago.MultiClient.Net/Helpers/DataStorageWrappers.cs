using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using System;
using System.Collections.Generic;

#if NET35
#else
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Methods for working with read-only data storage sets
	/// </summary>
	public interface IDataStorageWrapper
	{
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		Hint[] GetHints(int? slot = null, int? team = null);

#if NET35
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="onHintsRetrieved">the method to call with the retrieved hints</param>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		void GetHintsAsync(Action<Hint[]> onHintsRetrieved, int? slot = null, int? team = null);
#else
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		Task<Hint[]> GetHintsAsync(int? slot = null, int? team = null);
#endif

		/// <summary>
		/// Sets a callback to be called with all updates to the unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="onHintsUpdated">the method to call with the updated hints</param>
		/// <param name="retrieveCurrentlyUnlockedHints">should the currently unlocked hints be retrieved or just the updates</param>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		void TrackHints(Action<Hint[]> onHintsUpdated,
			bool retrieveCurrentlyUnlockedHints = true, int? slot = null, int? team = null);

		/// <summary>
		/// Retrieves the custom slot data for the specified slot
		/// </summary>
		/// <param name="slot">the slot id of the player to request slot data for, defaults to the current player's slot if left empty</param>
		/// <returns>An Dictionary with string keys, and custom defined values, the keys and values differ per game</returns>
		Dictionary<string, object> GetSlotData(int? slot = null);

#if NET35
		/// <summary>
		/// Retrieves the custom slot data for the specified slot
		/// </summary>
		/// <param name="onSlotDataRetrieved">the method to call with the retrieved slot data</param>
		/// <param name="slot">the slot id of the player to request slot data for, defaults to the current player's slot if left empty</param>
		/// <returns>An Dictionary with string keys, and custom defined values, the keys and values differ per game</returns>
		void GetSlotDataAsync(Action<Dictionary<string, object>> onSlotDataRetrieved, int? slot = null);
#else
		/// <summary>
		/// Retrieves the custom slot data for the specified slot
		/// </summary>
		/// <param name="slot">the slot id of the player to request slot data for, defaults to the current player's slot if left empty</param>
		/// <returns>An Dictionary with string keys, and custom defined values, the keys and values differ per game</returns>
		Task<Dictionary<string, object>> GetSlotDataAsync(int? slot = null);
#endif

		/// <summary>
		/// Retrieves the defined item name groups for the specified game
		/// </summary>
		/// <param name="game">the game name to request item name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with item group names for keys and an array of item names as value</returns>
		Dictionary<string, string[]> GetItemNameGroups(string game = null);

#if NET35
		/// <summary>
		/// Retrieves the defined item name groups for the specified game
		/// </summary>
		/// <param name="onItemNameGroupsRetrieved">the method to call with the retrieved item name groups</param>
		/// <param name="game">the game name to request item name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with item group names for keys and an array of item names as value</returns>
		void GetItemNameGroupsAsync(Action<Dictionary<string, string[]>> onItemNameGroupsRetrieved, string game = null);
#else
		/// <summary>
		/// Retrieves the defined item name groups for the specified game
		/// </summary>
		/// <param name="game">the game name to request item name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with item group names for keys and an array of item names as value</returns>
		Task<Dictionary<string, string[]>> GetItemNameGroupsAsync(string game = null);
#endif

		/// <summary>
		/// Retrieves the defined location name groups for the specified game
		/// </summary>
		/// <param name="game">the game name to request location name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with location group names for keys and an array of location names as value</returns>
		Dictionary<string, string[]> GetLocationNameGroups(string game = null);

#if NET35
		/// <summary>
		/// Retrieves the defined location name groups for the specified game
		/// </summary>
		/// <param name="onLocationNameGroupsRetrieved">the method to call with the retrieved location name groups</param>
		/// <param name="game">the game name to request location name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with location group names for keys and an array of location names as value</returns>
		void GetLocationNameGroupsAsync(Action<Dictionary<string, string[]>> onLocationNameGroupsRetrieved, string game = null);
#else
		/// <summary>
		/// Retrieves the defined location name groups for the specified game
		/// </summary>
		/// <param name="game">the game name to request location name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with location group names for keys and an array of location names as value</returns>
		Task<Dictionary<string, string[]>> GetLocationNameGroupsAsync(string game = null);
#endif

		/// <summary>
		/// Retrieves the client status for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request the status for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request the status for, defaults to the current player's team if left empty</param>
		/// <returns>The status of the client or null if the slot or team does not exist</returns>
		ArchipelagoClientState GetClientStatus(int? slot = null, int? team = null);

#if NET35
		/// <summary>
		/// Retrieves the client status for the specified player slot and team
		/// </summary>
		/// <param name="onStatusRetrieved">the method to call with the retrieved client status</param>
		/// <param name="slot">the slot id of the player to request the status for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request the status for, defaults to the current player's team if left empty</param>
		/// <returns>The status of the client or null if the slot or team does not exist</returns>
		void GetClientStatusAsync(Action<ArchipelagoClientState> onStatusRetrieved, int? slot = null, int? team = null);
#else
		/// <summary>
		/// Retrieves the client status for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request the status for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request the status for, defaults to the current player's team if left empty</param>
		/// <returns>The status of the client or null if the slot or team does not exist</returns>
		Task<ArchipelagoClientState> GetClientStatusAsync(int? slot = null, int? team = null);
#endif

		/// <summary>
		/// Sets a callback to be called with all updates to the client status for the specified player slot and team
		/// </summary>
		/// <param name="onStatusUpdated">the method to call with the updated hints</param>
		/// <param name="retrieveCurrentClientStatus">should the current status be retrieved or just the updates</param>
		/// <param name="slot">the slot id of the player to request the status for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request the status for, defaults to the current player's team if left empty</param>
		void TrackClientStatus(Action<ArchipelagoClientState> onStatusUpdated,
			bool retrieveCurrentClientStatus = true, int? slot = null, int? team = null);
	}

	public partial class DataStorageHelper : IDataStorageWrapper
	{
		DataStorageElement GetHintsElement(int? slot = null, int? team = null) =>
			this[Scope.ReadOnly, $"hints_{team ?? connectionInfoProvider.Team}_{slot ?? connectionInfoProvider.Slot}"];
		DataStorageElement GetSlotDataElement(int? slot = null) =>
			this[Scope.ReadOnly, $"slot_data_{slot ?? connectionInfoProvider.Slot}"];
		DataStorageElement GetItemNameGroupsElement(string game = null) =>
			this[Scope.ReadOnly, $"item_name_groups_{game ?? connectionInfoProvider.Game}"];
		DataStorageElement GetLocationNameGroupsElement(string game = null) =>
			this[Scope.ReadOnly, $"location_name_groups_{game ?? connectionInfoProvider.Game}"];
		DataStorageElement GetClientStatusElement(int? slot = null, int? team = null) =>
			this[Scope.ReadOnly, $"client_status_{team ?? connectionInfoProvider.Team}_{slot ?? connectionInfoProvider.Slot}"];

		/// <inheritdoc />
		public Hint[] GetHints(int? slot = null, int? team = null) => 
			GetHintsElement(slot, team).To<Hint[]>();
#if NET35
		/// <inheritdoc />
		public void GetHintsAsync(Action<Hint[]> onHintsRetrieved, int? slot = null, int? team = null) =>
			GetHintsElement(slot, team).GetAsync(t => onHintsRetrieved(t?.ToObject<Hint[]>()));
#else
		/// <inheritdoc />
		public Task<Hint[]> GetHintsAsync(int? slot = null, int? team = null) =>
			GetHintsElement(slot, team).GetAsync<Hint[]>();
#endif

		/// <inheritdoc />
		public void TrackHints(Action<Hint[]> onHintsUpdated, 
			bool retrieveCurrentlyUnlockedHints = true, int? slot = null, int? team = null)
		{
			GetHintsElement(slot, team).OnValueChanged += (_, newValue) => onHintsUpdated(newValue.ToObject<Hint[]>());

			if (retrieveCurrentlyUnlockedHints)
#if NET35
				GetHintsAsync(onHintsUpdated, slot, team);
#else
				GetHintsAsync(slot, team).ContinueWith(t => onHintsUpdated(t.Result));
#endif
		}

		/// <inheritdoc />
		public Dictionary<string, object> GetSlotData(int? slot = null) =>
			GetSlotDataElement(slot).To<Dictionary<string, object>>();
#if NET35
		/// <inheritdoc />
		public void GetSlotDataAsync(Action<Dictionary<string, object>> onSlotDataRetrieved, int? slot = null) =>
			GetSlotDataElement(slot).GetAsync(t => onSlotDataRetrieved(t?.ToObject<Dictionary<string, object>>()));
#else
		/// <inheritdoc />
		public Task<Dictionary<string, object>> GetSlotDataAsync(int? slot = null) =>
			GetSlotDataElement(slot).GetAsync<Dictionary<string, object>>();
#endif

		/// <inheritdoc />
		public Dictionary<string, string[]> GetItemNameGroups(string game = null) =>
			GetItemNameGroupsElement(game).To<Dictionary<string, string[]>>();

#if NET35
		/// <inheritdoc />
		public void GetItemNameGroupsAsync(Action<Dictionary<string, string[]>> onItemNameGroupsRetrieved, string game = null) =>
			GetItemNameGroupsElement(game).GetAsync(t => onItemNameGroupsRetrieved(t?.ToObject<Dictionary<string, string[]>>()));
#else
		/// <inheritdoc />
		public Task<Dictionary<string, string[]>> GetItemNameGroupsAsync(string game = null) =>
			GetItemNameGroupsElement(game).GetAsync<Dictionary<string, string[]>>();
#endif

		/// <inheritdoc />
		public Dictionary<string, string[]> GetLocationNameGroups(string game = null) =>
			GetLocationNameGroupsElement(game).To<Dictionary<string, string[]>>();

#if NET35
		/// <inheritdoc />
		public void GetLocationNameGroupsAsync(Action<Dictionary<string, string[]>> onLocationNameGroupsRetrieved, string game = null) =>
			GetLocationNameGroupsElement(game).GetAsync(t => onLocationNameGroupsRetrieved(t?.ToObject<Dictionary<string, string[]>>()));
#else
		/// <inheritdoc />
		public Task<Dictionary<string, string[]>> GetLocationNameGroupsAsync(string game = null) =>
			GetLocationNameGroupsElement(game).GetAsync<Dictionary<string, string[]>>();
#endif
		//TODO add unit tests

		/// <inheritdoc />
		public ArchipelagoClientState GetClientStatus(int? slot = null, int? team = null) =>
			GetClientStatusElement(slot, team).To<ArchipelagoClientState?>() ?? ArchipelagoClientState.ClientUnknown;
#if NET35
		/// <inheritdoc />
		public void GetClientStatusAsync(Action<ArchipelagoClientState> onStatusRetrieved, int? slot = null, int? team = null) =>
			GetClientStatusElement(slot, team).GetAsync(t => onStatusRetrieved(t.ToObject<ArchipelagoClientState?>() ?? ArchipelagoClientState.ClientUnknown));
#else
		/// <inheritdoc />
		public Task<ArchipelagoClientState> GetClientStatusAsync(int? slot = null, int? team = null) =>
			GetClientStatusElement(slot, team)
				.GetAsync<ArchipelagoClientState?>()
				.ContinueWith(r => r.Result ?? ArchipelagoClientState.ClientUnknown);
#endif

		/// <inheritdoc />
		public void TrackClientStatus(Action<ArchipelagoClientState> onStatusUpdated,
			bool retrieveCurrentClientStatus = true, int? slot = null, int? team = null)
		{
			GetClientStatusElement(slot, team).OnValueChanged += (_, newValue) => onStatusUpdated(newValue.ToObject<ArchipelagoClientState>());

			if (retrieveCurrentClientStatus)
#if NET35
				GetClientStatusAsync(onStatusUpdated, slot, team);
#else
				GetClientStatusAsync(slot, team).ContinueWith(t => onStatusUpdated(t.Result));
#endif
		}
	}
}
