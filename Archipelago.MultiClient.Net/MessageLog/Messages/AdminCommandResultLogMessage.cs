using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class AdminCommandResultLogMessage : LogMessage
	{
		internal AdminCommandResultLogMessage(MessagePart[] parts) : base(parts)
		{
		}
	}
}