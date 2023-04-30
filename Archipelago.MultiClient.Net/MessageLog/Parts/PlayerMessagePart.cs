using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	public class PlayerMessagePart : MessagePart
	{
		public bool IsActivePlayer { get; }
		public int SlotId { get; }
        
		internal PlayerMessagePart(IPlayerHelper players, IConnectionInfoProvider connectionInfo, JsonMessagePart part) : base (MessagePartType.Player, part)
		{
			switch (part.Type)
			{
				case JsonMessagePartType.PlayerId:
					SlotId = int.Parse(part.Text);
					IsActivePlayer = SlotId == connectionInfo.Slot;
					Text = players.GetPlayerAlias(SlotId) ?? $"Player {SlotId}";
					break;
				case JsonMessagePartType.PlayerName:
					SlotId = 0; // value is not slot resolvable according to docs
					IsActivePlayer = false;
					Text = part.Text;
					break;
			}

			Color = GetColor(IsActivePlayer);
		}

		static Color GetColor(bool isActivePlayer)
		{
			if (isActivePlayer)
				return Color.Magenta;

			return Color.Yellow;
		}
	}
}