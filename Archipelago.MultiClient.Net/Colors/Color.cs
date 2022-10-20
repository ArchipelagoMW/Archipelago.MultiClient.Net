using System;

namespace Archipelago.MultiClient.Net.Colors
{
    /// <summary>
    /// Polyfill for System.Drawing.Color for games on runtimes which do not supply a `System.Drawing.dll`.
    /// Not focused on memory or computation efficiency, we just need something that works reasonably well.
    /// Transparency/alpha channel is not handled.
    /// </summary>
    public partial struct Color : IEquatable<Color>
    {
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Color color &&
                   R == color.R &&
                   G == color.G &&
                   B == color.B;
        }

        public bool Equals(Color other)
{
            return R == other.R &&
                   G == other.G &&
                   B == other.B;
        }

        public override int GetHashCode()
        {
            int hashCode = -1520100960;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        public static explicit operator ConsoleColor(Color color)
        {
            if (consoleColorConversions.TryGetValue(color, out var consoleColor))
            {
                return consoleColor;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast Color(R={color.R},G={color.G},B={color.B}) to a ConsoleColor.");
            }
        }
    }
}
