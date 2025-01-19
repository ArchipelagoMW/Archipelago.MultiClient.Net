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
		/// The color corresponding to <see cref="PaletteColor"/> in the built-in dark palette.
		/// </summary>
		public Color Color => GetColor(BuiltInPalettes.Dark);

		/// <summary>
		/// The specified color for this message part, or null if not specified.
		/// </summary>
		public PaletteColor? PaletteColor { get; protected set; }

		/// <summary>
		/// Whether this message part's color is intended to represent a background color
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
				PaletteColor = ColorUtils.GetColor(messagePart.Color.Value);
				IsBackgroundColor = messagePart.Color.Value >= JsonMessagePartColor.BlackBg;
			}
			else
			{
				PaletteColor = null;
			}
		}

		/// <summary>
		/// Gets the color corresponding to <see cref="PaletteColor"/> in the specified palette.
		/// </summary>
		/// <param name="palette">The palette to retrieve the color from</param>
		public T GetColor<T>(Palette<T> palette) => palette[PaletteColor];

		/// <summary>
		/// The text to display of this message part
		/// </summary>
		public override string ToString() => Text;
	}
}