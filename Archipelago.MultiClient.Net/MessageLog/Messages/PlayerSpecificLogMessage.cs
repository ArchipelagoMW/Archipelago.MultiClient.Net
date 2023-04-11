using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class PlayerSpecificLogMessage : LogMessage
	{
		public PlayerInfo Player { get; }
		public bool IsActivePlayer { get; }
		public bool IsRelatedToActivePlayer { get; }

		internal PlayerSpecificLogMessage(MessagePart[] parts, 
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot)
			: base(parts)
		{
			var playerList = players.AllPlayers;

			IsActivePlayer = connectionInfo.Team == team && connectionInfo.Slot == slot;
			Player = (playerList.Count > slot) ? playerList[slot] : new PlayerInfo();
			IsRelatedToActivePlayer = IsActivePlayer || Player.IsSharingGroupWith(connectionInfo.Team, connectionInfo.Slot);
		}
	}
}