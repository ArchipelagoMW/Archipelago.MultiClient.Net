using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class CommandResultLogMessage : LogMessage
	{
		internal CommandResultLogMessage(MessagePart[] parts) : base(parts)
		{
		}
	}
}