using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.Colors
{
	/// <summary>
	/// Static utilities for getting the appropriate palette color from various protocol-provided information
	/// </summary>
	public static class ColorUtils
	{
		/// <summary>
		/// The color to be used to represent the currently connected player
		/// </summary>
		public const PaletteColor ActivePlayerColor = PaletteColor.Magenta;
		/// <summary>
		/// The color to be used to represent any player other the currently connected player
		/// </summary>
		public const PaletteColor NonActivePlayerColor = PaletteColor.Yellow;

		/// <summary>
		/// Gets the palette color corresponding to a JsonMessagePartColor.
		/// </summary>
		/// <returns>The corresponding palette color, or null if not specified</returns>
		public static PaletteColor? GetColor(JsonMessagePartColor color)
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

		/// <summary>
		/// Gets the palette color corresponding to a HintStatus.
		/// </summary>
		/// <returns>The corresponding palette color, or null if not specified</returns>
		public static PaletteColor? GetColor(HintStatus status)
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

		/// <summary>
		/// Gets the palette color corresponding to a Hint's status.
		/// </summary>
		/// <returns>The corresponding palette color, or null if not specified</returns>
		public static PaletteColor? GetColor(Hint hint)
		{
			return GetColor(hint.Status);
		}

		/// <summary>
		/// Gets the palette color corresponding to an ItemFlags.
		/// </summary>
		/// <returns>The corresponding palette color, or null if not specified</returns>
		public static PaletteColor? GetColor(ItemFlags flags)
		{
			if (HasFlag(flags, ItemFlags.Advancement))
				return PaletteColor.Plum;
			if (HasFlag(flags, ItemFlags.NeverExclude))
				return PaletteColor.SlateBlue;
			if (HasFlag(flags, ItemFlags.Trap))
				return PaletteColor.Salmon;

			return PaletteColor.Cyan;
		}

		/// <summary>
		/// Gets the palette color corresponding to an ItemInfo's flags.
		/// </summary>
		/// <returns>The corresponding palette color, or null if not specified</returns>
		public static PaletteColor? GetColor(ItemInfo item)
		{
			return GetColor(item.Flags);
		}

		/// <summary>
		/// Gets the palette color corresponding to a PlayerInfo.
		/// </summary>
		/// <returns>The corresponding palette color, or null if not specified</returns>
		public static PaletteColor? GetColor(PlayerInfo player, IConnectionInfoProvider connectionInfo)
		{
			bool isActivePlayer = player.Slot == connectionInfo.Slot;
			return isActivePlayer ? ActivePlayerColor : NonActivePlayerColor;
		}

		static bool HasFlag(ItemFlags flags, ItemFlags flag) =>
#if NET35
			(flags & flag) > 0;
#else
			flags.HasFlag(flag);
#endif
	}
}
