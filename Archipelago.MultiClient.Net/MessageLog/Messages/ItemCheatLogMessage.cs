using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class ItemCheatLogMessage : ItemSendLogMessage
	{
		internal ItemCheatLogMessage(MessagePart[] parts, 
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, 
			int team, int slot, NetworkItem item)
			: base(parts, players, connectionInfo, slot, -1, item, team)
		{
		}
	}
}