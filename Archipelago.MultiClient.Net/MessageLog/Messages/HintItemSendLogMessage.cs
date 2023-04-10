using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class HintItemSendLogMessage : ItemSendLogMessage
	{
		public bool IsFound { get; }

		internal HintItemSendLogMessage(MessagePart[] parts, int receiver, int sender, NetworkItem item, bool found) 
			: base(parts, receiver, sender, item)
		{
			IsFound = found;
		}
	}
}