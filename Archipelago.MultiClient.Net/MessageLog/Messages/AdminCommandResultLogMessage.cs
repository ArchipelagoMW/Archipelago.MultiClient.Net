using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class AdminCommandResultLogMessage : LogMessage
	{
		internal AdminCommandResultLogMessage(MessagePart[] parts) : base(parts)
		{
		}
	}
}