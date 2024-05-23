using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.MultiClient.Net.Models
{
	/// <summary>
	/// Information about an item and its location
	/// </summary>
	public class ItemInfo
	{
		readonly IReceivedItemsHelper receivedItemsHelper;
		readonly ILocationCheckHelper locationCheckHelper;

		/// <summary>
		/// The item id of the item. Item ids are in the range of ± (2^53)-1.
		/// </summary>
		public long ItemId { get; }

		/// <summary>
		/// The location id of the item inside the world. Location ids are in the range of ± (2^53)-1.
		/// </summary>
		public long LocationId { get; }

		/// <summary>
		/// The player of the world the item is located in
		/// </summary>
		public PlayerInfo Player { get; }

		/// <summary>
		/// Enum flags that describe special properties of the Item
		/// </summary>
		public ItemFlags Flags { get; }

		/// <summary>
		/// The name of the item
		/// </summary>
		public string ItemName => receivedItemsHelper.GetItemName(ItemId, Game);

		/// <summary>
		/// The name of the location that item is at
		/// </summary>
		public string LocationName => locationCheckHelper.GetLocationNameFromId(ItemId, Game);

		/// <summary>
		/// The game the item belongs to
		/// </summary>
		public string Game { get; }

		public ItemInfo(NetworkItem item, string game,
			IReceivedItemsHelper receivedItemsHelper, ILocationCheckHelper locationCheckHelper, PlayerInfo player)
		{
			this.receivedItemsHelper = receivedItemsHelper;
			this.locationCheckHelper = locationCheckHelper;

			Game = game;
			ItemId = item.Item;
			LocationId = item.Location;
			Flags = item.Flags;

			Player = player;
		}
	}

	/// <summary>
	/// Information about an item and its location
	/// </summary>
	public class ScoutedItemInfo : ItemInfo
	{
		/// <summary>
		/// The player to receive the item
		/// </summary>
		public new PlayerInfo Player { get; }

		public ScoutedItemInfo(NetworkItem item, string game, IReceivedItemsHelper receivedItemsHelper, ILocationCheckHelper locationCheckHelper, PlayerInfo player) 
			: base(item, game, receivedItemsHelper, locationCheckHelper, player)
		{
			Player = player;
		}
	}
}
