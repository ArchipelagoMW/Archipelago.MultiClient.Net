using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class HintItemSendLogMessage : ItemSendLogMessage
	{
		public bool IsFound { get; }

		internal HintItemSendLogMessage(MessagePart[] parts, 
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, 
			int receiver, int sender, NetworkItem item, bool found) 
			: base(parts, players, connectionInfo, receiver, sender, item)
		{
			IsFound = found;
		}
	}
}