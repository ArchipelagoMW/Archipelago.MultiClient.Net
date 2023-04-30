using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An command result message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `CommandResultLogMessage` is send in response to a client using an `!` command (e.g. !status)
	/// </summary>
	public class CommandResultLogMessage : LogMessage
	{
		internal CommandResultLogMessage(MessagePart[] parts) : base(parts)
		{
		}
	}
}