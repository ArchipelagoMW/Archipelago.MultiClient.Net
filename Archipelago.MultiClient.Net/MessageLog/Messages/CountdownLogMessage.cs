using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// A countdown message to display to the user, consisting of an array of message parts to form a sentence
	/// countdown message also contains the remaining seconds for the countdown for more specific processing
	/// </summary>
	public class CountdownLogMessage : LogMessage
	{
		public int RemainingSeconds { get; }

		internal CountdownLogMessage(MessagePart[] parts, int remainingSeconds) : base(parts)
		{
			RemainingSeconds = remainingSeconds;
		}
	}
}