using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	/// <summary>
	/// Part of a LogMessage that contains information about the entrance
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.MessagePart"/>
	public class EntranceMessagePart : MessagePart
	{
		internal EntranceMessagePart(JsonMessagePart messagePart) : base(MessagePartType.Entrance, messagePart, Colors.PaletteColor.Blue)
		{
			Text = messagePart.Text;
		}
	}
}