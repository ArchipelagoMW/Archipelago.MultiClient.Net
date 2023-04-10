using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class JoinLogMessage : LogMessage
	{
		public int Team { get; }
		public int Slot { get; }
		public string[] Tags { get; }

		internal JoinLogMessage(MessagePart[] parts, int team, int slot, string[] tags) : base(parts)
		{
			Team = team;
			Slot = slot;
			Tags = tags;
		}
	}
}