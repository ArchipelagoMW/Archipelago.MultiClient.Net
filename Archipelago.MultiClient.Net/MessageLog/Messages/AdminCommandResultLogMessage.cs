using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An admin command result message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `AdminCommandResultLogMessage` is send in response to a client using `!admin` command
	/// </summary>
	public class AdminCommandResultLogMessage : LogMessage
	{
		internal AdminCommandResultLogMessage(MessagePart[] parts) : base(parts)
		{
		}
	}
}