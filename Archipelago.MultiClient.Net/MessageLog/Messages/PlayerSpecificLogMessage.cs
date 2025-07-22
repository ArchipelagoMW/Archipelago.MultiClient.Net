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
		PlayerInfo ActivePlayer { get; }

		/// <summary>
		/// The player information about the player this message is concerning
		/// </summary>
		public PlayerInfo Player { get; }

		/// <summary>
		/// True if the player this message is concerning is the current connected player
		/// </summary>
		public bool IsActivePlayer => Player == ActivePlayer;

		/// <summary>
		/// True if the player this message is concerning any slot groups (e.g. itemlinks) with the current connected player
		/// </summary>
		public bool IsRelatedToActivePlayer => ActivePlayer.IsRelatedTo(Player);

		internal PlayerSpecificLogMessage(MessagePart[] parts, 
			IPlayerHelper players, int team, int slot)
			: base(parts)
		{
			ActivePlayer = players.ActivePlayer ?? new PlayerInfo();
			Player = players.GetPlayerInfo(team, slot) ?? new PlayerInfo();
		}
	}
}