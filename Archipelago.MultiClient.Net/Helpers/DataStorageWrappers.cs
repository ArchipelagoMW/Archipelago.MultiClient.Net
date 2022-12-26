using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

#if NET35
using System;
#else
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
	public partial class DataStorageHelper
	{
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		public Hint[] GetHints(int? slot = null, int? team = null) => 
			this[Scope.ReadOnly, $"hints_{team ?? connectionInfoProvider.Team}_{slot ?? connectionInfoProvider.Slot}"].To<Hint[]>();
#if NET35
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="onHintsRetrieved">the method to call with the retrieved hints</param>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		public void GetHintsAsync(Action<Hint[]> onHintsRetrieved, int? slot = null, int? team = null) => 
			GetAsync(Scope.ReadOnly, $"hints_{team ?? connectionInfoProvider.Team}_{slot ?? connectionInfoProvider.Slot}",
				t => onHintsRetrieved(t?.ToObject<Hint[]>()));
#else
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		public Task<Hint[]> GetHintsAsync(int? slot = null, int? team = null) =>
			GetAsync(Scope.ReadOnly, $"hints_{team ?? connectionInfoProvider.Team}_{slot ?? connectionInfoProvider.Slot}")
				.ContinueWith(t => t.Result?.ToObject<Hint[]>());
#endif

		/// <summary>
		/// Retrieves the custom slot data for the specified slot
		/// </summary>
		/// <param name="slot">the slot id of the player to request slot data for, defaults to the current player's slot if left empty</param>
		/// <returns>An Dictionary with string keys, and custom defined values, the keys and values differ per game</returns>
		public Dictionary<string, object> GetSlotData(int? slot = null) => 
			this[Scope.ReadOnly, $"slot_data_{slot ?? connectionInfoProvider.Slot}"].To<Dictionary<string, object>>();
#if NET35
		/// <summary>
		/// Retrieves the custom slot data for the specified slot
		/// </summary>
		/// <param name="onSlotDataRetrieved">the method to call with the retrieved slot data</param>
		/// <param name="slot">the slot id of the player to request slot data for, defaults to the current player's slot if left empty</param>
		/// <returns>An Dictionary with string keys, and custom defined values, the keys and values differ per game</returns>
		public void GetSlotDataAsync(Action<Dictionary<string, object>> onSlotDataRetrieved, int? slot = null) =>
			GetAsync(Scope.ReadOnly, $"slot_data_{slot ?? connectionInfoProvider.Slot}",
				t => onSlotDataRetrieved(t?.ToObject<Dictionary<string, object>>()));
#else
		/// <summary>
		/// Retrieves the custom slot data for the specified slot
		/// </summary>
		/// <param name="slot">the slot id of the player to request slot data for, defaults to the current player's slot if left empty</param>
		/// <returns>An Dictionary with string keys, and custom defined values, the keys and values differ per game</returns>
		public Task<Dictionary<string, object>> GetSlotDataAsync(int? slot = null) =>
			GetAsync(Scope.ReadOnly, $"slot_data_{slot ?? connectionInfoProvider.Slot}")
				.ContinueWith(t => t.Result?.ToObject<Dictionary<string, object>>());
#endif

		/// <summary>
		/// Retrieves the defined item name groups for the specified game
		/// </summary>
		/// <param name="game">the game name to request item name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with item group names for keys and an array of item names as value</returns>
		public Dictionary<string, string[]> GetItemNameGroups(string game = null) => 
			this[Scope.ReadOnly, $"item_name_groups_{game ?? connectionInfoProvider.Game}"].To<Dictionary<string, string[]>>();

#if NET35
		/// <summary>
		/// Retrieves the defined item name groups for the specified game
		/// </summary>
		/// <param name="onItemNameGroupsRetrieved">the method to call with the retrieved item name groups</param>
		/// <param name="game">the game name to request item name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with item group names for keys and an array of item names as value</returns>
		public void GetItemNameGroupsAsync(Action<Dictionary<string, string[]>> onItemNameGroupsRetrieved, string game = null) =>
			GetAsync(Scope.ReadOnly, $"item_name_groups_{game ?? connectionInfoProvider.Game}",
				t => onItemNameGroupsRetrieved(t?.ToObject<Dictionary<string, string[]>>()));
#else
		/// <summary>
		/// Retrieves the defined item name groups for the specified game
		/// </summary>
		/// <param name="game">the game name to request item name groups for, defaults to the current player's game if left empty</param>
		/// <returns>An Dictionary with item group names for keys and an array of item names as value</returns>
		public Task<Dictionary<string, string[]>> GetItemNameGroupsAsync(string game = null) =>
			GetAsync(Scope.ReadOnly, $"item_name_groups_{game ?? connectionInfoProvider.Game}")
				.ContinueWith(t => t.Result?.ToObject<Dictionary<string, string[]>>());
#endif
	}
}
