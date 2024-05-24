using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// The `PlayerSpecificLogMessage` is send a base class for LogMessage's that are made in the context of a specific player.
	/// Item player specific messages contain additional information about the specific player.
	///
	/// `ItemSendLogMessage` also serves as the base class for <see cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.HintItemSendLogMessage"/> and <see cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.ItemCheatLogMessage"/>
	/// </summary>
	public abstract class PlayerSpecificLogMessage : LogMessage
	{
		/// <summary>
		/// The player information about the player this message is concerning
		/// </summary>
		public PlayerInfo Player { get; }

		/// <summary>
		/// True if the player this message is concerning is the current connected player
		/// </summary>
		public bool IsActivePlayer { get; }

		/// <summary>
		/// True if the player this message is concerning shares any slot groups (e.g. itemlinks) with the current connected player
		/// </summary>
		public bool IsRelatedToActivePlayer { get; }

		internal PlayerSpecificLogMessage(MessagePart[] parts, 
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot)
			: base(parts)
		{
			var playerList = players.Players;

			IsActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == slot;

			Player = playerList.Count > team && playerList[team].Count > slot
				? playerList[team][slot]
				: new PlayerInfo();

			IsRelatedToActivePlayer = IsActivePlayer || Player.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot);
		}
	}
}