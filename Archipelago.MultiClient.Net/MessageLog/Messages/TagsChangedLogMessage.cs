using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class TagsChangedLogMessage : PlayerSpecificLogMessage
	{

		public string[] Tags { get; }

		internal TagsChangedLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot, string[] tags)
			: base(parts, players, connectionInfo, team, slot)
		{
			Tags = tags;
		}
	}
}