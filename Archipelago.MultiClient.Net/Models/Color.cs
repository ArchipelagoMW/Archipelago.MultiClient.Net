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
	    public static Color Red = new Color(255, 0, 0);
		public static Color Green = new Color(0, 128, 0);
		public static Color Yellow = new Color(255, 255, 0);
		public static Color Blue = new Color(0, 0, 255);
		public static Color Magenta = new Color(255, 0, 255);
		public static Color Cyan = new Color(0, 255, 255);
		public static Color Black = new Color(0, 0, 0);
		public static Color White = new Color(255, 255, 255);
		public static Color SlateBlue = new Color(106, 90, 205);
		public static Color Salmon = new Color(250, 128, 114);
		public static Color Plum = new Color(221, 160, 221);

	    public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }

		public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

		public override bool Equals(object obj) =>
	        obj is Color color &&
	        R == color.R &&
	        G == color.G &&
	        B == color.B;

        public bool Equals(Color other) =>
	        R == other.R &&
	        G == other.G &&
	        B == other.B;

        public override int GetHashCode()
        {
            var hashCode = -1520100960;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Color left, Color right) => left.Equals(right);

        public static bool operator !=(Color left, Color right) => !(left == right);
	}
}
