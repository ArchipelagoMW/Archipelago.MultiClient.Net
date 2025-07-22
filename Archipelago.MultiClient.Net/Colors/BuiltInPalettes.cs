using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Colors
{
	/// <summary>
	/// Contains built-in palettes for color handling
	/// </summary>
	public static class BuiltInPalettes
	{
		/// <summary>
		/// Default dark palette based on terminal colors
		/// </summary>
		public static readonly Palette<Color> Dark = new Palette<Color>(Color.White, new Dictionary<PaletteColor, Color>()
		{
			[PaletteColor.White] = Color.White,
			[PaletteColor.Black] = Color.Black,
			[PaletteColor.Red] = Color.Red,
			[PaletteColor.Green] = Color.Green,
			[PaletteColor.Blue] = Color.Blue,
			[PaletteColor.Cyan] = Color.Cyan,
			[PaletteColor.Magenta] = Color.Magenta,
			[PaletteColor.Yellow] = Color.Yellow,
			[PaletteColor.SlateBlue] = Color.SlateBlue,
			[PaletteColor.Salmon] = Color.Salmon,
			[PaletteColor.Plum] = Color.Plum,
		});
	}
}
