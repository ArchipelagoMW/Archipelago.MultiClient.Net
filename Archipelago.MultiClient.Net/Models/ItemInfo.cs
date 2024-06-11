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
		/// The name of the item, null if the name cannot be resolved
		/// </summary>
		public string ItemName => itemInfoResolver.GetItemName(ItemId, ItemGame);

		/// <summary>
		/// The name of the item for display purposes, returns a fallback string containing the ItemId if the name cannot be resolved 
		/// </summary>
		public string ItemDisplayName => ItemName ?? $"Item: {ItemId}";

		/// <summary>
		/// The name of the location that item is at, null if the name cannot be resolved
		/// </summary>
		public string LocationName => itemInfoResolver.GetLocationName(LocationId, LocationGame);

		/// <summary>
		/// The name of the location for display purposes, returns a fallback string containing the LocationId if the name cannot be resolved 
		/// </summary>
		public string LocationDisplayName => LocationName ?? $"Location: {LocationId}";

		/// <summary>
		/// The game the item belongs to
		/// </summary>
		public string ItemGame { get; }

		/// <summary>
		/// The game the location belongs to
		/// </summary>
		public string LocationGame { get; }

		/// <summary>
		/// The constructor what else did you expect it to be
		/// </summary>
		public ItemInfo(NetworkItem item, string receiverGame, string senderGame, IItemInfoResolver itemInfoResolver, PlayerInfo player)
			: this(item.Item, item.Location, item.Flags, receiverGame, senderGame, itemInfoResolver, player)
		{
		}

		/// <summary>
		/// The other constructor
		/// </summary>
		public ItemInfo(long itemId, long locationId, ItemFlags flags, string receiverGame, string senderGame, IItemInfoResolver itemInfoResolver, PlayerInfo player)
		{
			this.itemInfoResolver = itemInfoResolver;

			ItemGame = receiverGame;
			LocationGame = senderGame;
			ItemId = itemId;
			LocationId = locationId;
			Flags = flags;
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

	public class HintItemInfo : ItemInfo
	{
		/// <summary>
		/// The player of the world the item belongs to
		/// </summary>
		public PlayerInfo ReceivingPlayer { get; }

		/// <summary>
		/// The player of the world the item is located in
		/// </summary>
		public PlayerInfo FindingPlayer => Player;

		/// <summary>
		/// Whether the location has been marked as checked
		/// </summary>
		public bool Found { get; }

		/// <summary>
		/// The name of the entrance of the location
		/// </summary>
		public string Entrance { get; }

		/// <summary>
		/// The constructor what else did you expect it to be
		/// </summary>
		public HintItemInfo(Hint hint, IItemInfoResolver itemInfoResolver, PlayerInfo findingPlayer, PlayerInfo receivingPlayer)
			: base(hint.ItemId, hint.LocationId, hint.ItemFlags, receivingPlayer.Game, findingPlayer.Game, itemInfoResolver, findingPlayer)
		{
			Found = hint.Found;
			Entrance = hint.Entrance;
		}
	}
}
