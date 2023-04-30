using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using System;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An item send message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `ItemSendLogMessage` is send in response to a client obtaining an item.
	/// Item send messages contain additional information about the item that was sent for more specific processing.
	///
	/// `ItemSendLogMessage` also serves as the base class for <see cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.HintItemSendLogMessage"/> & <see cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.ItemCheatLogMessage"/>
	/// </summary>
	public class ItemSendLogMessage : LogMessage
	{
		/// <summary>
		/// The player slot number of the player who received the item
		/// </summary>
		[Obsolete("Use Receiver.Slot instead")]
		public int ReceivingPlayerSlot { get; }

		/// <summary>
		/// The player slot number of the player who sent the item
		/// </summary>
		[Obsolete("Use Sender.Slot instead")]
		public int SendingPlayerSlot { get; }
		
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
		public NetworkItem Item { get; }

		internal ItemSendLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo,
			int receiver, int sender, NetworkItem item) 
			: this(parts, players, connectionInfo, receiver, sender, item, connectionInfo.Team)
		{
		}

		internal ItemSendLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo,
			int receiver, int sender, NetworkItem item, int team) : base(parts)
		{
			var playerList = players.Players;

			ReceivingPlayerSlot = receiver;
			SendingPlayerSlot = sender;

			IsReceiverTheActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == receiver;
			IsSenderTheActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == sender;

			Receiver = playerList.Count > team && playerList[team].Count > receiver
				? playerList[team][receiver]
				: new PlayerInfo();
			Sender = playerList.Count > team && playerList[team].Count > sender
				? playerList[team][sender]
				: new PlayerInfo();

			IsRelatedToActivePlayer = IsReceiverTheActivePlayer || IsSenderTheActivePlayer
				|| Receiver.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot)
				|| Sender.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot);

			Item = item;
		}
	}
}