using Archipelago.MultiClient.Net.Colors;
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
		public Color Color => GetColor(BuiltInPalettes.Dark);

		public PaletteColor? PaletteColor { get; protected set; }

		/// <summary>
		/// The specified background color for this message part
		/// </summary>
		public bool IsBackgroundColor { get; internal set; }

		internal MessagePart(MessagePartType type, JsonMessagePart messagePart, PaletteColor? color = null)
		{
			Type = type;
			Text = messagePart.Text;

			if (color.HasValue)
			{
				PaletteColor = color.Value;
			}
			else if (messagePart.Color.HasValue)
			{
				PaletteColor = ColorUtils.GetMessagePartColor(messagePart.Color.Value);
				IsBackgroundColor = messagePart.Color.Value >= JsonMessagePartColor.BlackBg;
			}
			else
			{
				PaletteColor = null;
			}
		}

		public T GetColor<T>(Palette<T> palette) => palette[PaletteColor];

		/// <summary>
		/// The text to display of this message part
		/// </summary>
		public override string ToString() => Text;
	}
}