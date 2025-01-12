using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Colors
{
	// this implementation with a default color allows partial implementations for custom palettes,
	// as well as the ability to add new PaletteColors without breaking any palettes - if you had
	// to specify all palette colors to create a palette, the addition of a new color is breaking
	// in both source and binary, while this is neither.
	public class Palette<T>
	{
		private Dictionary<PaletteColor, T> palette;

		public T DefaultColor { get; private set; }

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

		public Palette(T defaultColor, Dictionary<PaletteColor, T> palette)
		{
			this.DefaultColor = defaultColor;
			this.palette = new Dictionary<PaletteColor, T>(palette);
		}

		// allows transforming an existing palette to a new data type (the use case treble mentioned)
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

		// allows overriding part of a palette, for use cases like "I like this palette but this one color is a problem to display"
		public Palette<T> Edit(T newDefault, Dictionary<PaletteColor, T> paletteEdits)
		{
			Dictionary<PaletteColor, T> newPalette = new Dictionary<PaletteColor, T>(palette);
			foreach (KeyValuePair<PaletteColor, T> kv in paletteEdits)
			{
				newPalette[kv.Key] = kv.Value;
			}
			return new Palette<T>(newDefault, newPalette);
		}

		// convenience overload of above preserving default color
		public Palette<T> Edit(Dictionary<PaletteColor, T> paletteEdits)
		{
			return Edit(DefaultColor, paletteEdits);
		}
	}
}
