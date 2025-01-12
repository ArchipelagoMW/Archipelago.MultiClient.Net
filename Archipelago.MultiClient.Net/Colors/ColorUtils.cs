using Archipelago.MultiClient.Net.Enums;

namespace Archipelago.MultiClient.Net.Colors
{
	public static class ColorUtils
	{
		public static PaletteColor? GetMessagePartColor(JsonMessagePartColor color)
		{
			switch (color)
			{
				case JsonMessagePartColor.Red:
				case JsonMessagePartColor.RedBg:
					return PaletteColor.Red;
				case JsonMessagePartColor.Green:
				case JsonMessagePartColor.GreenBg:
					return PaletteColor.Green;
				case JsonMessagePartColor.Yellow:
				case JsonMessagePartColor.YellowBg:
					return PaletteColor.Yellow;
				case JsonMessagePartColor.Blue:
				case JsonMessagePartColor.BlueBg:
					return PaletteColor.Blue;
				case JsonMessagePartColor.Magenta:
				case JsonMessagePartColor.MagentaBg:
					return PaletteColor.Magenta;
				case JsonMessagePartColor.Cyan:
				case JsonMessagePartColor.CyanBg:
					return PaletteColor.Cyan;
				case JsonMessagePartColor.Black:
				case JsonMessagePartColor.BlackBg:
					return PaletteColor.Black;
				case JsonMessagePartColor.White:
				case JsonMessagePartColor.WhiteBg:
					return PaletteColor.White;
				default:
					return null;
			}
		}

		public static PaletteColor? GetHintColor(HintStatus status)
		{
			switch (status)
			{
				case HintStatus.Found:
					return PaletteColor.Green;
				case HintStatus.NoPriority:
					return PaletteColor.SlateBlue;
				case HintStatus.Avoid:
					return PaletteColor.Salmon;
				case HintStatus.Priority:
					return PaletteColor.Plum;
				default:
					return null;
			}
		}

		public static PaletteColor GetItemColor(ItemFlags flags)
		{
			if (HasFlag(flags, ItemFlags.Advancement))
				return PaletteColor.Plum;
			if (HasFlag(flags, ItemFlags.NeverExclude))
				return PaletteColor.SlateBlue;
			if (HasFlag(flags, ItemFlags.Trap))
				return PaletteColor.Salmon;

			return PaletteColor.Cyan;
		}

		public static PaletteColor? GetPlayerColor(bool isActivePlayer)
		{
			if (isActivePlayer)
				return PaletteColor.Magenta;

			return PaletteColor.Yellow;
		}

		static bool HasFlag(ItemFlags flags, ItemFlags flag) =>
#if NET35
			(flags & flag) > 0;
#else
			flags.HasFlag(flag);
#endif
	}
}
