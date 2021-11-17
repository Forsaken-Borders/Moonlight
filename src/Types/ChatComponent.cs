using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

namespace Moonlight.Types
{
    public class ChatComponent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("bold")]
        public bool Bold { get; set; }

        [JsonPropertyName("italic")]
        public bool Italic { get; set; }

        [JsonPropertyName("underlined")]
        public bool Underlined { get; set; }

        [JsonPropertyName("strikethrough")]
        public bool Strikethrough { get; set; }

        [JsonPropertyName("obfuscated")]
        public bool Obfuscated { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("insertation")]
        public string Insertation { get; set; }

        [JsonPropertyName("extra")]
        public ChatComponent[] Extra { get; set; }

        public ChatComponent(string text) => Text = text;

        public static implicit operator ChatComponent(string text) => new(text);

        public static string AmpersandToSectionSign(string str)
        {
            StringBuilder stringBuilder = new(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '&')
                {
                    // Check if the next character is a ampersand and replace it with §, while making sure we don't go out of bounds
                    if ((i + 1) < str.Length && str[i + 1] == '&')
                    {
                        stringBuilder.Append('&');
                        i++;
                    }
                    else
                    {
                        stringBuilder.Append('§');
                    }
                }
                else
                {
                    stringBuilder.Append(str[i]);
                }
            }

            return stringBuilder.ToString();
        }

        public string ToString(bool hexColorCodes = false)
        {
            StringBuilder stringBuilder = new();
            if (Bold)
            {
                stringBuilder.Append("§l");
            }

            if (Italic)
            {
                stringBuilder.Append("§o");
            }

            if (Underlined)
            {
                stringBuilder.Append("§n");
            }

            if (Strikethrough)
            {
                stringBuilder.Append("§m");
            }

            if (Obfuscated)
            {
                stringBuilder.Append("§k");
            }

            switch (Color)
            {
                case "dark_red":
                case "#AA0000":
                case "\u00A74":
                    stringBuilder.Append("§4");
                    break;
                case "red":
                case "#FF5555":
                case "\u00A7c":
                    stringBuilder.Append("§c");
                    break;
                case "gold":
                case "#FFAA00":
                case "\u00A76":
                    stringBuilder.Append("§6");
                    break;
                case "yellow":
                case "#FFFF55":
                case "\u00A7e":
                    stringBuilder.Append("§e");
                    break;
                case "green":
                case "#55FF55":
                case "\u00A7a":
                    stringBuilder.Append("§a");
                    break;
                case "dark_green":
                case "#00AA00":
                case "\u00A72":
                    stringBuilder.Append("§2");
                    break;
                case "aqua":
                case "#55FFFF":
                case "\u00A7b":
                    stringBuilder.Append("§b");
                    break;
                case "dark_aqua":
                case "#00AAAA":
                case "\u00A73":
                    stringBuilder.Append("§3");
                    break;
                case "dark_blue":
                case "#0000AA":
                case "\u00A71":
                    stringBuilder.Append("§1");
                    break;
                case "blue":
                case "#5555FF":
                case "\u00A79":
                    stringBuilder.Append("§9");
                    break;
                case "light_purple":
                case "#FF55FF":
                case "\u00A7d":
                    stringBuilder.Append("§d");
                    break;
                case "dark_purple":
                case "#AA00AA":
                case "\u00A75":
                    stringBuilder.Append("§5");
                    break;
                case "white":
                case "#FFFFFF":
                case "\u00A7f":
                    stringBuilder.Append("§f");
                    break;
                case "grey":
                case "gray":
                case "#AAAAAA":
                case "\u00A77":
                    stringBuilder.Append("§7");
                    break;
                case "dark_grey":
                case "dark_gray":
                case "#555555":
                case "\u00A78":
                    stringBuilder.Append("§8");
                    break;
                case "black":
                case "#000000":
                case "\u00A70":
                    stringBuilder.Append("§0");
                    break;
                default:
                    if (hexColorCodes)
                    {
                        if (ColorTranslator.FromHtml(Color) != null)
                        {
                            stringBuilder.Append("§#");
                            stringBuilder.Append(Color[0] == '#' ? Color.AsSpan(1) : Color); // Remove the #, if it exists.
                        }
                        else
                        {
                            throw new FormatException($"Invalid color code: {Color}");
                        }
                    } // if hexColorCodes is false, ignore the custom color.
                    break;
            }

            stringBuilder.Append(AmpersandToSectionSign(Text));
            foreach (ChatComponent chatComponent in Extra)
            {
                stringBuilder.Append(chatComponent.ToString());
            }

            return stringBuilder.ToString();
        }

        public override bool Equals(object obj) => obj is ChatComponent component && Text == component.Text && Bold == component.Bold && Italic == component.Italic && Underlined == component.Underlined && Strikethrough == component.Strikethrough && Obfuscated == component.Obfuscated && Color == component.Color && Insertation == component.Insertation && EqualityComparer<ChatComponent[]>.Default.Equals(Extra, component.Extra);

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Text);
            hash.Add(Bold);
            hash.Add(Italic);
            hash.Add(Underlined);
            hash.Add(Strikethrough);
            hash.Add(Obfuscated);
            hash.Add(Color);
            hash.Add(Insertation);
            hash.Add(Extra);
            return hash.ToHashCode();
        }
    }
}