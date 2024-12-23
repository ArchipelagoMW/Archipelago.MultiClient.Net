using Archipelago.MultiClient.Net.Enums;
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

			switch (messagePart.HintStatus)
			{
				case HintStatus.Found:
					Color = Color.Green;
					break;
				case HintStatus.Unspecified:
					Color = Color.White;
					break;
				case HintStatus.NoPriority:
					Color = Color.SlateBlue;
					break;
				case HintStatus.Avoid:
					Color = Color.Salmon;
					break;
				case HintStatus.Priority:
					Color = Color.Plum;
					break;
			}
		}
	}
}