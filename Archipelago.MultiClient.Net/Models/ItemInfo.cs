using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.MultiClient.Net.Models
{
	/// <summary>
	/// Information about an item and its location
	/// </summary>
	public class ItemInfo
	{
		readonly IItemInfoResolver itemInfoResolver;

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
		public string ItemName => itemInfoResolver.GetItemName(ItemId, Game);

		/// <summary>
		/// The name of the location that item is at
		/// </summary>
		public string LocationName => itemInfoResolver.GetLocationName(ItemId, Game);

		/// <summary>
		/// The game the item belongs to
		/// </summary>
		public string Game { get; }

		/// <summary>
		/// The game the location belongs to
		/// </summary>
		public string LocationGame { get; }

		/// <summary>
		/// The constructor what else did you expect it to be
		/// </summary>
		public ItemInfo(NetworkItem item, string receiverGame, string senderGame, IItemInfoResolver itemInfoResolver, PlayerInfo player)
		{
			this.itemInfoResolver = itemInfoResolver;

			Game = receiverGame;
			LocationGame = senderGame;
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
		public new PlayerInfo Player => base.Player;

		/// <summary>
		/// The constructor what else did you expect it to be
		/// </summary>
		public ScoutedItemInfo(NetworkItem item, string receiverGame, string senderGame, IItemInfoResolver itemInfoResolver, PlayerInfo player) 
			: base(item, receiverGame, senderGame, itemInfoResolver, player)
		{
		}
	}
}
