
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Helpers
{
	public partial class DataStorageHelper
	{
		/// <summary>
		/// TODO DOCS
		/// </summary>
		/// <returns></returns>
		public Hint[] GetHints() => GetHints(connectionInfoProvider.Slot);
		public Hint[] GetHints(int slot) => GetHints(connectionInfoProvider.Team, slot);
		public Hint[] GetHints(int team, int slot) => 
			this[Scope.ReadOnly, $"hints_{team}_{slot}"].To<Hint[]>();


		public Dictionary<string, object> GetSlotData() => GetSlotData(connectionInfoProvider.Slot);
		public Dictionary<string, object> GetSlotData(int slot) => 
			this[Scope.ReadOnly, $"slot_data_{slot}"].To<Dictionary<string, object>>();


		public Dictionary<string, string[]> GetItemNameGroups() => GetItemNameGroups(connectionInfoProvider.Game);
		public Dictionary<string, string[]> GetItemNameGroups(string game) => 
			this[Scope.ReadOnly, $"item_name_groups_{game}"].To<Dictionary<string, string[]>>();
	}
}
