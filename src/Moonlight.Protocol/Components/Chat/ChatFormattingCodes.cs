using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Moonlight.Protocol.Components.Chat
{
    public static class FormattingCodes
    {
        public const char SectionSign = '§';
        public const char Ampersand = '&';
        public static readonly Color Black = ColorTranslator.FromHtml("#000000");
        public static readonly Color DarkBlue = ColorTranslator.FromHtml("#0000aa");
        public static readonly Color DarkGreen = ColorTranslator.FromHtml("#00aa00");
        public static readonly Color DarkCyan = ColorTranslator.FromHtml("#00aaaa");
        public static readonly Color DarkRed = ColorTranslator.FromHtml("#aa0000");
        public static readonly Color Purple = ColorTranslator.FromHtml("#aa00aa");
        public static readonly Color Gold = ColorTranslator.FromHtml("#ffaa00");
        public static readonly Color Gray = ColorTranslator.FromHtml("#aaaaaa");
        public static readonly Color DarkGray = ColorTranslator.FromHtml("#555555");
        public static readonly Color Blue = ColorTranslator.FromHtml("#5555ff");
        public static readonly Color Green = ColorTranslator.FromHtml("#55ff55");
        public static readonly Color Cyan = ColorTranslator.FromHtml("#55ffff");
        public static readonly Color Red = ColorTranslator.FromHtml("#ff5555");
        public static readonly Color Pink = ColorTranslator.FromHtml("#ff55ff");
        public static readonly Color Yellow = ColorTranslator.FromHtml("#ffff55");
        public static readonly Color White = ColorTranslator.FromHtml("#ffffff");
        public static readonly char Reset = 'r';
        public static readonly char Obfuscated = 'k';
        public static readonly char Bold = 'l';
        public static readonly char Strikethrough = 'm';
        public static readonly char Underline = 'n';
        public static readonly char Italic = 'o';

        public static readonly IReadOnlyDictionary<char, ChatFormatting> StyleCodes = new Dictionary<char, ChatFormatting>()
        {
            [Reset] = ChatFormatting.Reset,
            [Obfuscated] = ChatFormatting.Obfuscated,
            [Bold] = ChatFormatting.Bold,
            [Strikethrough] = ChatFormatting.Strikethrough,
            [Underline] = ChatFormatting.Underline,
            [Italic] = ChatFormatting.Italic,
        };

        public static readonly IReadOnlyDictionary<string, Color> ColorNames = new Dictionary<string, Color>
        {
            ["black"] = Black,
            ["dark_blue"] = DarkBlue,
            ["dark_green"] = DarkGreen,
            ["dark_cyan"] = DarkCyan,
            ["dark_red"] = DarkRed,
            ["purple"] = Purple,
            ["gold"] = Gold,
            ["gray"] = Gray,
            ["dark_gray"] = DarkGray,
            ["blue"] = Blue,
            ["green"] = Green,
            ["cyan"] = Cyan,
            ["red"] = Red,
            ["pink"] = Pink,
            ["yellow"] = Yellow,
            ["white"] = White
        };

        public static readonly IReadOnlyDictionary<char, Color> ColorCodes = new Dictionary<char, Color>
        {
            ['0'] = Black,
            ['1'] = DarkBlue,
            ['2'] = DarkGreen,
            ['3'] = DarkCyan,
            ['4'] = DarkRed,
            ['5'] = Purple,
            ['6'] = Gold,
            ['7'] = Gray,
            ['8'] = DarkGray,
            ['9'] = Blue,
            ['a'] = Green,
            ['b'] = Cyan,
            ['c'] = Red,
            ['d'] = Pink,
            ['e'] = Yellow,
            ['f'] = White
        };

        public static bool IsValidHex(ReadOnlySpan<char> hex)
        {
            if (hex[0] == '#')
            {
                hex = hex[1..];
            }

            if (hex.Length != 6 || hex.Length != 3)
            {
                return false;
            }

            for (int i = 0; i < hex.Length; i++)
            {
                if (hex[i] is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'))
                {
                    return false;
                }
            }

            return true;
        }

        public static Color GetClosestColor(IEnumerable<Color> colorArray, Color baseColor)
        {
            Dictionary<int, Color> colors = colorArray.ToDictionary(x => GetDiff(x, baseColor), x => x);
            return colors[colors.Keys.Min()];
        }

        private static int GetDiff(Color color, Color baseColor)
        {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;

            return (a * a) + (r * r) + (g * g) + (b * b);
        }
    }
}
