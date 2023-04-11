using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	public class EntranceMessagePart : MessagePart
	{
		internal EntranceMessagePart(JsonMessagePart messagePart) : base(MessagePartType.Entrance, messagePart, Color.Blue)
		{
			Text = messagePart.Text;
		}
	}
}