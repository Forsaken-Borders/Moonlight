using System.Drawing;
using System.Linq;

namespace Moonlight.Types.Chat;

public static class ChatColors
{
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

    public static Color GetColor(string color) => color switch
    {
        "black" => Black,
        "dark_blue" => DarkBlue,
        "dark_green" => DarkGreen,
        "dark_cyan" => DarkCyan,
        "dark_red" => DarkRed,
        "purple" => Purple,
        "gold" => Gold,
        "gray" => Gray,
        "dark_gray" => DarkGray,
        "blue" => Blue,
        "green" => Green,
        "cyan" => Cyan,
        "red" => Red,
        "pink" => Pink,
        "yellow" => Yellow,
        "white" => White,
        _ => Color.Empty
    };

    public static Color GetColor(char color) => color switch
    {
        '0' => Black,
        '1' => DarkBlue,
        '2' => DarkGreen,
        '3' => DarkCyan,
        '4' => DarkRed,
        '5' => Purple,
        '6' => Gold,
        '7' => Gray,
        '8' => DarkGray,
        '9' => Blue,
        'a' => Green,
        'b' => Cyan,
        'c' => Red,
        'd' => Pink,
        'e' => Yellow,
        'f' => White,
        _ => Color.Empty
    };


    public static string GetColorWord(Color color) => ColorTranslator.ToHtml(color) switch
    {
        "#000000" => "black",
        "#0000aa" => "dark_blue",
        "#00aa00" => "dark_green",
        "#00aaaa" => "dark_cyan",
        "#aa0000" => "dark_red",
        "#aa00aa" => "purple",
        "#ffaa00" => "gold",
        "#aaaaaa" => "gray",
        "#555555" => "dark_gray",
        "#5555ff" => "blue",
        "#55ff55" => "green",
        "#55ffff" => "cyan",
        "#ff5555" => "red",
        "#ff55ff" => "pink",
        "#ffff55" => "yellow",
        "#ffffff" => "white",
        _ => " "
    };

    public static char GetColorCode(Color color) => ColorTranslator.ToHtml(color) switch
    {
        "#000000" => '0',
        "#0000AA" => '1',
        "#00AA00" => '2',
        "#00AAAA" => '3',
        "#AA0000" => '4',
        "#AA00AA" => '5',
        "#FFAA00" => '6',
        "#AAAAAA" => '7',
        "#555555" => '8',
        "#5555FF" => '9',
        "#55FF55" => 'a',
        "#55FFFF" => 'b',
        "#FF5555" => 'c',
        "#FF55FF" => 'd',
        "#FFFF55" => 'e',
        "#FFFFFF" => 'f',
        _ => ' '
    };

    public static Color[] GetColors() => new[] {
            Black,
            DarkBlue,
            DarkGreen,
            DarkCyan,
            DarkRed,
            Purple,
            Gold,
            Gray,
            DarkGray,
            Blue,
            Green,
            Cyan,
            Red,
            Pink,
            Yellow,
            White
        };

    public static char[] GetColorCodes() => new[] {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'a',
            'b',
            'c',
            'd',
            'e',
            'f'
        };

    public static bool IsValidHex(string hexString)
    {
        if (hexString.StartsWith('#'))
        {
            hexString = hexString.Remove(0, 1);
        }
        return (hexString.Length == 6 || hexString.Length == 3) && hexString.All(c => c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'));
    }

    public static Color GetClosestColor(Color[] colorArray, Color baseColor)
    {
        var colors = colorArray.Select(x => new { Value = x, Diff = GetDiff(x, baseColor) }).ToList();
        int min = colors.Min(x => x.Diff);
        return colors.Find(x => x.Diff == min).Value;
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
