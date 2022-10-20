using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Colors
{
    public partial struct Color
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

        private static Dictionary<Color, ConsoleColor> consoleColorConversions = new Dictionary<Color, ConsoleColor>()
        {
            [Red] = ConsoleColor.Red,
            [Green] = ConsoleColor.Green,
            [Yellow] = ConsoleColor.Yellow,
            [Blue] = ConsoleColor.Blue,
            [Magenta] = ConsoleColor.Magenta,
            [Cyan] = ConsoleColor.Cyan,
            [Black] = ConsoleColor.Black,
            [White] = ConsoleColor.White,
        };
    }
}
