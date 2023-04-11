using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class ReleaseLogMessage : PlayerSpecificLogMessage
	{
		internal ReleaseLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot)
			: base(parts, players, connectionInfo, team, slot)
		{
		}
	}
}