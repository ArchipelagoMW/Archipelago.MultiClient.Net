using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class ChatLogMessage : PlayerSpecificLogMessage
	{
		public string Message { get; }

		internal ChatLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot, string message) 
			: base(parts, players, connectionInfo, team, slot)
		{
			Message = message;
		}
	}
}