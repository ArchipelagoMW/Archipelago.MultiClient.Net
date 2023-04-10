using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class TagsChangedLogMessage : LogMessage
	{
		public string[] Tags { get; }

		internal TagsChangedLogMessage(MessagePart[] parts, string[] tags) : base(parts)
		{
			Tags = tags;
		}
	}
}