using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using System;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class ItemSendLogMessage : LogMessage
	{
		[Obsolete("Use Receiver.Slot instead")]
		public int ReceivingPlayerSlot { get; }
		[Obsolete("Use Sender.Slot instead")]
		public int SendingPlayerSlot { get; }

		public PlayerInfo Receiver { get; }
		public PlayerInfo Sender { get; }
		public bool IsReceiverTheActivePlayer { get; }
		public bool IsSenderTheActivePlayer { get; }
		public bool IsRelatedToActivePlayer { get; }
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
			var playerList = players.AllPlayers;

			ReceivingPlayerSlot = receiver;
			SendingPlayerSlot = sender;

			IsReceiverTheActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == receiver;
			IsSenderTheActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == sender;

			Receiver = (playerList.Count > receiver && receiver > 0) ? playerList[receiver] : new PlayerInfo();
			Sender = (playerList.Count > sender && sender > 0) ? playerList[sender] : new PlayerInfo();

			IsRelatedToActivePlayer = IsReceiverTheActivePlayer || IsSenderTheActivePlayer
				|| Receiver.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot)
				|| Sender.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot);

			Item = item;
		}
	}
}