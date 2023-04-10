using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class ItemCheatLogMessage : ItemSendLogMessage
	{
		public int Team { get; }

		internal ItemCheatLogMessage(MessagePart[] parts, int receiver, int sender, NetworkItem item, int team)
			: base(parts, receiver, sender, item)
		{
			Team = team;
		}
	}
}