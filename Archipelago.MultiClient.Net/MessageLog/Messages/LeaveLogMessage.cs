using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class LeaveLogMessage : PlayerSpecificLogMessage
	{
		internal LeaveLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot)
			: base(parts, players, connectionInfo, team, slot)
		{
		}
	}
}