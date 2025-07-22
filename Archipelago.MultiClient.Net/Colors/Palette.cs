using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Colors
{
	/// <summary>
	/// Palette/theme support for client colors
	/// </summary>
	/// <typeparam name="T">
	/// The type of actual color in the palette, typically an engine- or framework-specific Color struct
	/// </typeparam>
	public class Palette<T>
	{
		private readonly Dictionary<PaletteColor, T> palette;

		/// <summary>
		/// The default color to use in the palette when color is not specified, or the requested color is not
		/// in the palette
		/// </summary>
		public T DefaultColor { get; private set; }

		/// <summary>
		/// Retrieves a color from the palette
		/// </summary>
		/// <param name="color">The palette color to look up</param>
		/// <returns>
		/// The requested color. Returns the default color if the requested color was null or
		/// not defined in this palette.
		/// </returns>
		public T this[PaletteColor? color]
		{
			get
			{
				if (color.HasValue && palette.TryGetValue(color.Value, out T result))
				{
					return result;
				}
				return DefaultColor;
			}
		}

		/// <summary>
		/// Creates a custom palette
		/// </summary>
		/// <param name="defaultColor">The default color for the palette</param>
		/// <param name="palette">
		/// The colors in the palette. Note that this does not need to define a mapping for every
		/// PaletteColor if it is not desired.
		/// </param>
		public Palette(T defaultColor, Dictionary<PaletteColor, T> palette)
		{
			this.DefaultColor = defaultColor;
			this.palette = new Dictionary<PaletteColor, T>(palette);
		}

		/// <summary>
		/// Creates a copy of this palette with all values transformed to another type
		/// </summary>
		/// <typeparam name="U">The desired color type for the new palette</typeparam>
		/// <param name="transform">A transformation function to convert to the new type</param>
		public Palette<U> Transform<U>(Func<T, U> transform)
		{
			U newDefault = transform(DefaultColor);
			Dictionary<PaletteColor, U> newPalette = new Dictionary<PaletteColor, U>(palette.Count);
			foreach (KeyValuePair<PaletteColor, T> kv in palette)
			{
				newPalette[kv.Key] = transform(kv.Value);
			}
			return new Palette<U>(newDefault, newPalette);
		}

		/// <summary>
		/// Creates a copy of this palette with the specified colors replaced
		/// </summary>
		/// <param name="newDefault">The new default color</param>
		/// <param name="paletteEdits">A mapping specifying colors to replace</param>
		public Palette<T> Edit(T newDefault, Dictionary<PaletteColor, T> paletteEdits)
		{
			Dictionary<PaletteColor, T> newPalette = new Dictionary<PaletteColor, T>(palette);
			foreach (KeyValuePair<PaletteColor, T> kv in paletteEdits)
			{
				newPalette[kv.Key] = kv.Value;
			}
			return new Palette<T>(newDefault, newPalette);
		}

		/// <summary>
		/// Creates a copy of this palette with the specified colors replaced. The default color
		/// is preserved.
		/// </summary>
		/// <param name="paletteEdits">A mapping specifying colors to replace</param>
		public Palette<T> Edit(Dictionary<PaletteColor, T> paletteEdits)
		{
			return Edit(DefaultColor, paletteEdits);
		}
	}
}
