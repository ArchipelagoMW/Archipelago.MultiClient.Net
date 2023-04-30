using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class CommandResultLogMessage : LogMessage
	{
		internal CommandResultLogMessage(MessagePart[] parts) : base(parts)
		{
		}
	}
}