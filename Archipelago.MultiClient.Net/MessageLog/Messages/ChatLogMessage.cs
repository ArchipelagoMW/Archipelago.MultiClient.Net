using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.Helpers
{
	public class ChatLogMessage : LogMessage
	{
		public int Team { get; }
		public int Slot { get; }
		public string Message { get; }

		internal ChatLogMessage(MessagePart[] parts, int team, int slot, string message) : base(parts)
		{
			Team = team;
			Slot = slot;
			Message = message;
		}
	}
}