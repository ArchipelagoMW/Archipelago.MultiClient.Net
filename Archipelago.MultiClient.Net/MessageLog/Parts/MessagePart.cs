using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	/// <summary>
	/// Part of a LogMessage that contains information on how to display the piece of text
	/// </summary>
	public class MessagePart
	{
		/// <summary>
		/// The text to display
		/// </summary>
		public string Text { get; internal set; }

		/// <summary>
		/// The type of message part
		/// </summary>
		public MessagePartType Type { get; internal set; }

		/// <summary>
		/// The specified or default color for this message part
		/// </summary>
		public Color Color { get; internal set; }

		/// <summary>
		/// The specified background color for this message part
		/// </summary>
		public bool IsBackgroundColor { get; internal set; }

		internal MessagePart(MessagePartType type, JsonMessagePart messagePart, Color? color = null)
		{
			Type = type;
			Text = messagePart.Text;

			if (color.HasValue)
			{
				Color = color.Value;
			}
			else if (messagePart.Color.HasValue)
			{
				Color = GetColor(messagePart.Color.Value);
				IsBackgroundColor = messagePart.Color.Value >= JsonMessagePartColor.BlackBg;
			}
			else
			{
				Color = Color.White;
			}
		}

		static Color GetColor(JsonMessagePartColor color)
		{
			switch (color)
			{
				case JsonMessagePartColor.Red:
				case JsonMessagePartColor.RedBg:
					return Color.Red;
				case JsonMessagePartColor.Green:
				case JsonMessagePartColor.GreenBg:
					return Color.Green;
				case JsonMessagePartColor.Yellow:
				case JsonMessagePartColor.YellowBg:
					return Color.Yellow;
				case JsonMessagePartColor.Blue:
				case JsonMessagePartColor.BlueBg:
					return Color.Blue;
				case JsonMessagePartColor.Magenta:
				case JsonMessagePartColor.MagentaBg:
					return Color.Magenta;
				case JsonMessagePartColor.Cyan:
				case JsonMessagePartColor.CyanBg:
					return Color.Cyan;
				case JsonMessagePartColor.Black:
				case JsonMessagePartColor.BlackBg:
					return Color.Black;
				case JsonMessagePartColor.White:
				case JsonMessagePartColor.WhiteBg:
					return Color.White;
				default:
					return Color.White;
			}
		}

		/// <summary>
		/// The text to display of this message part
		/// </summary>
		public override string ToString() => Text;
	}
}