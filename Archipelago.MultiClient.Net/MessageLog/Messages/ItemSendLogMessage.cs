using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An item send message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `ItemSendLogMessage` is send in response to a client obtaining an item.
	/// Item send messages contain additional information about the item that was sent for more specific processing.
	///
	/// `ItemSendLogMessage` also serves as the base class for <see cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.HintItemSendLogMessage"/> and <see cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.ItemCheatLogMessage"/>
	/// </summary>
	public class ItemSendLogMessage : LogMessage
	{
		/// <summary>
		/// The player who received the item
		/// </summary>
		public PlayerInfo Receiver { get; }

		/// <summary>
		/// The player who sent the item
		/// </summary>
		public PlayerInfo Sender { get; }

		/// <summary>
		/// Checks if the Receiver is the current connected player
		/// </summary>
		public bool IsReceiverTheActivePlayer { get; }

		/// <summary>
		/// True if the Sender is the current connected player
		/// </summary>
		public bool IsSenderTheActivePlayer { get; }

		/// <summary>
		/// True if either the Receiver or Sender share any slot groups (e.g. itemlinks) with the current connected player
		/// </summary>
		public bool IsRelatedToActivePlayer { get; }

		/// <summary>
		/// The Item that was send
		/// </summary>
		public ItemInfo Item { get; }

		internal ItemSendLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo,
			int receiver, int sender, NetworkItem item, IItemInfoResolver itemInfoResolver)
			: this(parts, players, connectionInfo, receiver, sender, item, connectionInfo.Team, itemInfoResolver)
		{
		}

		internal ItemSendLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo,
			int receiver, int sender, NetworkItem item, int team,
			IItemInfoResolver itemInfoResolver) : base(parts)
		{
			IsReceiverTheActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == receiver;
			IsSenderTheActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == sender;

			Receiver = players.GetPlayerInfo(team, receiver) ?? new PlayerInfo();
			Sender = players.GetPlayerInfo(team, sender) ?? new PlayerInfo();
			var itemPlayer = players.GetPlayerInfo(team, item.Player) ?? new PlayerInfo();
			
			IsRelatedToActivePlayer = IsReceiverTheActivePlayer || IsSenderTheActivePlayer
				|| Receiver.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot)
				|| Sender.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot);

			Item = new ItemInfo(item, Receiver.Game, Sender.Game, itemInfoResolver, itemPlayer);
		}
	}
}