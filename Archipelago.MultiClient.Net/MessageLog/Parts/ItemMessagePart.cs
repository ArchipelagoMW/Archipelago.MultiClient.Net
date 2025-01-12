using Archipelago.MultiClient.Net.Colors;
using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	/// <summary>
	/// Part of a LogMessage that contains information about an item
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.MessagePart"/>
	public class ItemMessagePart : MessagePart
	{
		/// <summary>
		/// The flags that identify the items its special properties
		/// </summary>
		public ItemFlags Flags { get; }
		
		/// <summary>
		/// The id of the item
		/// </summary>
		public long ItemId { get; }

		/// <summary>
		/// The player that owns the item (receiver)
		/// </summary>
		public int Player { get; }

		internal ItemMessagePart(IPlayerHelper players, IItemInfoResolver items, JsonMessagePart part) : base(MessagePartType.Item, part)
		{
			Flags = part.Flags ?? ItemFlags.None;
			PaletteColor = ColorUtils.GetItemColor(Flags);
			Player = part.Player ?? 0;

			var game = (players.GetPlayerInfo(Player) ?? new PlayerInfo()).Game;

			switch (part.Type)
			{
				case JsonMessagePartType.ItemId:
					ItemId = long.Parse(part.Text);
					Text = items.GetItemName(ItemId, game) ?? $"Item: {ItemId}";
					break; 
				case JsonMessagePartType.ItemName:
					ItemId = 0; // we are not going to try to reverse lookup this value based on the game of the receiving player
					Text = part.Text;
					break;
			}
		}
	}
}