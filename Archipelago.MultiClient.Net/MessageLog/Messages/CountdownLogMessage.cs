using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class CountdownLogMessage : LogMessage
	{
		public int RemainingSeconds { get; }

		internal CountdownLogMessage(MessagePart[] parts, int remainingSeconds) : base(parts)
		{
			RemainingSeconds = remainingSeconds;
		}
	}
}