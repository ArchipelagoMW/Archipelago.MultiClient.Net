using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class ServerChatLogMessage : LogMessage
	{
		public string Message { get; }

		internal ServerChatLogMessage(MessagePart[] parts, string message) : base(parts)
		{
			Message = message;
		}
	}
}