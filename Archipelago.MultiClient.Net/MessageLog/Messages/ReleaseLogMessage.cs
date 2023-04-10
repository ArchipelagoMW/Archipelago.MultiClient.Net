using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class ReleaseLogMessage : LogMessage
	{
		public int Team { get; }
		public int Slot { get; }

		internal ReleaseLogMessage(MessagePart[] parts, int team, int slot) : base(parts)
		{
			Team = team;
			Slot = slot;
		}
	}
}