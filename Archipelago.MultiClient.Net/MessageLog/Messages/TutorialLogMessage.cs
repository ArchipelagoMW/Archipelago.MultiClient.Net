using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An tutorial message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `TutorialLogMessage` is send in response to a client joining a multiworld (e.g. "Now that you are connected...")
	/// </summary>
	public class TutorialLogMessage : LogMessage
	{
		internal TutorialLogMessage(MessagePart[] parts) : base(parts)
		{
		}
	}
}