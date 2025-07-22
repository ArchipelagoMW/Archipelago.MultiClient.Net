using Archipelago.MultiClient.Net.Colors;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	/// <summary>
	/// Part of a LogMessage that contains information about the entrance
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.MessagePart"/>
	public class HintStatusMessagePart : MessagePart
	{
		internal HintStatusMessagePart(JsonMessagePart messagePart) : base(MessagePartType.HintStatus, messagePart)
		{
			Text = messagePart.Text;

			if (messagePart.HintStatus.HasValue)
			{
				PaletteColor = ColorUtils.GetColor(messagePart.HintStatus.Value);
			}
		}
	}
}