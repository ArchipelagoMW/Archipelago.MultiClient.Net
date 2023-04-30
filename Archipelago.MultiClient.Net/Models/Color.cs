using System;

namespace Archipelago.MultiClient.Net.Models
{
    /// <summary>
    /// Polyfill for System.Drawing.Color for games on runtimes which do not supply a `System.Drawing.dll`.
    /// Not focused on memory or computation efficiency, we just need something that works reasonably well.
    /// Transparency/alpha channel is not handled.
    /// </summary>
    public struct Color : IEquatable<Color>
    {
	    /// <summary>
	    /// Predefined Archipelago Color Red (used for not found hints)
	    /// </summary>
	    public static Color Red = new Color(255, 0, 0);
	    /// <summary>
	    /// Predefined Archipelago Color Green (used for found hints)
	    /// </summary>
		public static Color Green = new Color(0, 128, 0);
	    /// <summary>
	    /// Predefined Archipelago Color Yellow (used for the local player)
	    /// </summary>
		public static Color Yellow = new Color(255, 255, 0);
	    /// <summary>
	    /// Predefined Archipelago Color Blue (used for entrances)
	    /// </summary>
		public static Color Blue = new Color(0, 0, 255);
	    /// <summary>
	    /// Predefined Archipelago Color Magenta (used for other players)
	    /// </summary>
		public static Color Magenta = new Color(255, 0, 255);
	    /// <summary>
	    /// Predefined Archipelago Color Cyan (used for normal items)
	    /// </summary>
		public static Color Cyan = new Color(0, 255, 255);
	    /// <summary>
	    /// Predefined Archipelago Color Black
	    /// </summary>
		public static Color Black = new Color(0, 0, 0);
	    /// <summary>
	    /// Predefined Archipelago Color White (default text color)
	    /// </summary>
		public static Color White = new Color(255, 255, 255);
		/// <summary>
		/// Predefined Archipelago Color SlateBlue (used for important items)
		/// </summary>
		public static Color SlateBlue = new Color(106, 90, 205);
		/// <summary>
		/// Predefined Archipelago Color Salmon (used for traps)
		/// </summary>
		public static Color Salmon = new Color(250, 128, 114);
		/// <summary>
		/// Predefined Archipelago Color Plum (used for progression item)
		/// </summary>
		public static Color Plum = new Color(221, 160, 221);

	    /// <summary>
	    /// Red
	    /// </summary>
	    public byte R { get; set; }
		/// <summary>
		/// Green
		/// </summary>
		public byte G { get; set; }
		/// <summary>
		/// Blue
		/// </summary>
		public byte B { get; set; }

		/// <summary>
		/// A color
		/// </summary>
		/// <param name="r">Red</param>
		/// <param name="g">Green</param>
		/// <param name="b">Blue</param>
		public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

		/// <inheritdoc />
		public override bool Equals(object obj) =>
	        obj is Color color &&
	        R == color.R &&
	        G == color.G &&
	        B == color.B;

		/// <inheritdoc />
		public bool Equals(Color other) =>
	        R == other.R &&
	        G == other.G &&
	        B == other.B;

		/// <inheritdoc />
		public override int GetHashCode()
        {
            var hashCode = -1520100960;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Value Equality
        /// </summary>
        /// <param name="left">a color</param>
        /// <param name="right">a color</param>
        /// <returns>true, if both sides represent the same color</returns>
        public static bool operator ==(Color left, Color right) => left.Equals(right);

        /// <summary>
        /// Value In-Equality
        /// </summary>
        /// <param name="left">a color</param>
        /// <param name="right">a color</param>
        /// <returns>true, if both sides represent an different color</returns>
		public static bool operator !=(Color left, Color right) => !(left == right);
	}
}
